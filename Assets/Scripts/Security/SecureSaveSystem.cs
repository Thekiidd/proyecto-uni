using System;
using System.IO;
using UnityEngine;

namespace Platformer.Security
{
    public enum SaveLoadStatus
    {
        Success,
        FileNotFound,
        IntegrityCompromised,
        CorruptedFormat,
        Error
    }

    /// <summary>
    /// Sistema de guardado y carga seguro con cifrado AES-256,
    /// verificación de firma SHA-256 y respaldo automático contra corrupción o trampas.
    /// </summary>
    public static class SecureSaveSystem
    {
        private const uint FileMagicNumber = 0x53474F44; // "DOGS" en little-endian
        private const byte CurrentFileVersion = 1;
        private const string PrimarySaveFileName = "savegame.dat";
        private const string BackupSaveFileName = "savegame.bak";

        private static string SaveDirectory => Application.persistentDataPath;
        private static string PrimarySavePath => Path.Combine(SaveDirectory, PrimarySaveFileName);
        private static string BackupSavePath => Path.Combine(SaveDirectory, BackupSaveFileName);

        /// <summary>
        /// Guarda los datos cifrados y firmados con SHA-256.
        /// </summary>
        public static bool Save(GameSaveData data)
        {
            try
            {
                if (data == null) return false;
                data.saveTimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                string json = JsonUtility.ToJson(data);
                byte[] cipherBytes = EncryptionService.EncryptStringToBytes(json);
                byte[] shaHash = EncryptionService.ComputeSha256(cipherBytes); // 32 bytes

                // Si existe el archivo actual, se copia a backup primero
                if (File.Exists(PrimarySavePath))
                {
                    try { File.Copy(PrimarySavePath, BackupSavePath, true); } catch { }
                }

                // Estructura del archivo binario:
                // [4B Magic] + [1B Version] + [32B SHA-256 Hash] + [NB CipherPayload]
                using (var fs = new FileStream(PrimarySavePath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(fs))
                {
                    writer.Write(FileMagicNumber);
                    writer.Write(CurrentFileVersion);
                    writer.Write(shaHash);
                    writer.Write(cipherBytes.Length);
                    writer.Write(cipherBytes);
                }

                Debug.Log($"[SecureSaveSystem] Partida guardada con éxito (AES-256 + SHA-256) en: {PrimarySavePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SecureSaveSystem] Error al guardar partida: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carga y verifica la integridad de la partida.
        /// </summary>
        public static GameSaveData Load(out SaveLoadStatus status)
        {
            var data = TryLoadFile(PrimarySavePath, out status);
            if (status == SaveLoadStatus.Success)
            {
                return data;
            }

            // Si falla la integridad o el formato, intentar restaurar el backup
            if (status == SaveLoadStatus.IntegrityCompromised || status == SaveLoadStatus.CorruptedFormat)
            {
                Debug.LogWarning($"[SecureSaveSystem] ⚠️ Archivo principal comprometido ({status}). Intentando restaurar desde respaldo...");
                if (File.Exists(BackupSavePath))
                {
                    var backupData = TryLoadFile(BackupSavePath, out var backupStatus);
                    if (backupStatus == SaveLoadStatus.Success)
                    {
                        Debug.Log("[SecureSaveSystem] ✅ Respaldo restaurado con integridad válida.");
                        // Reemplazar archivo principal corrupto con el backup válido
                        Save(backupData);
                        status = SaveLoadStatus.Success;
                        return backupData;
                    }
                }
            }

            // Si no hay partida o fue imposible recuperar, crear estado nuevo inicial
            if (status == SaveLoadStatus.FileNotFound)
            {
                Debug.Log("[SecureSaveSystem] No se encontró partida previa. Inicializando nuevo perfil seguro.");
            }
            else
            {
                Debug.LogError($"[SecureSaveSystem] No fue posible recuperar la partida ({status}). Creando partida segura en blanco.");
            }

            var defaultData = new GameSaveData();
            Save(defaultData);
            return defaultData;
        }

        private static GameSaveData TryLoadFile(string path, out SaveLoadStatus status)
        {
            if (!File.Exists(path))
            {
                status = SaveLoadStatus.FileNotFound;
                return null;
            }

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    if (fs.Length < 4 + 1 + 32 + 4)
                    {
                        status = SaveLoadStatus.CorruptedFormat;
                        return null;
                    }

                    uint magic = reader.ReadUInt32();
                    if (magic != FileMagicNumber)
                    {
                        status = SaveLoadStatus.CorruptedFormat;
                        return null;
                    }

                    byte version = reader.ReadByte();
                    byte[] storedHash = reader.ReadBytes(32);
                    int payloadLength = reader.ReadInt32();

                    if (payloadLength <= 0 || payloadLength > 10 * 1024 * 1024) // Límite de seguridad de 10MB
                    {
                        status = SaveLoadStatus.CorruptedFormat;
                        return null;
                    }

                    byte[] cipherBytes = reader.ReadBytes(payloadLength);

                    // 1. Verificación de Integridad Criptográfica (SHA-256)
                    byte[] computedHash = EncryptionService.ComputeSha256(cipherBytes);
                    if (!EncryptionService.ConstantTimeAreEqual(storedHash, computedHash))
                    {
                        Debug.LogError("[SecureSaveSystem] 🚨 ALERTA DE SEGURIDAD: El hash SHA-256 no coincide. El archivo fue modificado externamente.");
                        status = SaveLoadStatus.IntegrityCompromised;
                        return null;
                    }

                    // 2. Descifrado seguro AES-256
                    string json = EncryptionService.DecryptStringFromBytes(cipherBytes);
                    var data = JsonUtility.FromJson<GameSaveData>(json);

                    status = SaveLoadStatus.Success;
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SecureSaveSystem] Excepción al leer archivo de guardado: {ex.Message}");
                status = SaveLoadStatus.Error;
                return null;
            }
        }
    }
}
