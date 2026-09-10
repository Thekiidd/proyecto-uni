using System.Text.RegularExpressions;

namespace Platformer.Security
{
    /// <summary>
    /// Utilidad para sanitizar entradas de texto de usuario (nombres de perro, perfiles, etc.),
    /// previniendo inyecciones de código, caracteres de control y desbordamientos.
    /// </summary>
    public static class InputSanitizer
    {
        private static readonly Regex SafePattern = new Regex(@"[^a-zA-Z0-9 áéíóúÁÉÍÓÚñÑüÜ_-]", RegexOptions.Compiled);

        /// <summary>
        /// Limpia un texto permitiendo únicamente caracteres alfanuméricos en español, espacios, guiones y longitud máxima.
        /// </summary>
        public static string SanitizeText(string input, int maxLength = 16, string defaultValue = "Amigo")
        {
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            // Eliminar espacios al inicio y final
            string cleaned = input.Trim();

            // Filtrar caracteres peligrosos o no permitidos
            cleaned = SafePattern.Replace(cleaned, "");

            // Recortar si excede la longitud permitida
            if (cleaned.Length > maxLength)
                cleaned = cleaned.Substring(0, maxLength);

            return string.IsNullOrWhiteSpace(cleaned) ? defaultValue : cleaned;
        }
    }
}
