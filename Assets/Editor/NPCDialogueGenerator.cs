using System.IO;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Genera los NPCDialogueData ScriptableObjects con diálogos completos para cada NPC de la aldea.
    /// Menú: Tools > Village > Generate NPC Dialogues
    /// </summary>
    public static class NPCDialogueGenerator
    {
        private const string DIALOGUE_FOLDER = "Assets/Data/Dialogues/Village";

        [MenuItem("Tools/Village/Generate NPC Dialogues")]
        public static void GenerateAllDialogues()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Dialogues"))
                AssetDatabase.CreateFolder("Assets/Data", "Dialogues");
            if (!AssetDatabase.IsValidFolder(DIALOGUE_FOLDER))
                AssetDatabase.CreateFolder("Assets/Data/Dialogues", "Village");

            CreateDialogue("Sabio_Eldor",
                npcName:    "Sabio Eldor",
                profession: "Archivista del Reino",
                nameColor:  new Color(0.6f, 0.8f, 1f),
                greetings: new[] {
                    "¡Ah, por fin llegas! Estaba rezando para que alguien valiente apareciera.",
                    "Los Cristales de Luz han desaparecido y con ellos nuestra protección contra la oscuridad.",
                    "Hay cinco cristales dispersos por el mundo: el Bosque Eterno, las Ruinas del Norte, la Cueva de Cristal, el Pico Nevado y la Ciudad Perdida.",
                    "Solo alguien con el corazón puro de un perro puede recuperarlos. ¿Aceptas este destino?"
                },
                generalLines: new[] {
                    "Los Cristales de Luz emiten un brillo especial que solo los perros con sensibilidad mágica pueden detectar.",
                    "¿Cómo va tu búsqueda? Recuerda que el tiempo corre — la oscuridad avanza desde el este.",
                    "Si encuentras gemas azules en tu camino, guárdalas. Son fragmentos de cristal que amplifican tus poderes."
                },
                loreLines: new[] {
                    "Hace 500 años, el Gran Mago Canis creó los Cristales de Luz para proteger a todos los animales del reino.",
                    "Los cristales no son solo piedras — son la esencia condensada de la amistad entre perros y humanos.",
                    "La leyenda dice que quien reúna los cinco cristales podrá hacer un deseo para todo el reino.",
                    "Los ancestros de esta aldea eran guardianes de los cristales. Esa responsabilidad ahora recae en ti."
                },
                tipLines: new[] {
                    "Consejo: usa el olfato — los cristales emiten un olor dulce. Cuando sientas algo especial, ¡excava!",
                    "Los enemigos del bosque son débiles al agua. Si encuentras un estanque cerca, úsalo a tu favor.",
                    "Recuerda: tu cola puede ser un arma. Un giro rápido puede desorientar a los slimes.",
                    "Guarda energía para los jefes de zona. No gastes todas tus estrellas en enemigos pequeños."
                },
                dangerLines: new[] {
                    "¡Ten cuidado! Los exploradores que fueron antes que tú no regresaron.",
                    "La oscuridad se está expandiendo más rápido de lo que esperábamos. ¡Date prisa!"
                },
                farewells: new[] {
                    "¡Ve con el viento a tu favor, noble guardián! El reino depende de ti.",
                    "Regresa cuando tengas noticias. Estaré aquí, estudiando los mapas antiguos."
                },
                questOffer:         "Recupera el primer Cristal de Luz del Bosque Eterno. El camino está al oeste, tras el portón.",
                questActive:        "¿Encontraste el cristal del Bosque Eterno? Recuerda: brilla azul cuando está cerca.",
                questComplete:      "¡Lo lograste! Este cristal está cargado de energía pura. El reino te lo agradece. Aquí tienes tu recompensa.",
                questCoins:         10,
                hasQuest:           true
            );

            CreateDialogue("Aldeana_Maya",
                npcName:    "Maya",
                profession: "Tejedora",
                nameColor:  new Color(1f, 0.75f, 0.85f),
                greetings: new[] {
                    "¡Oh, un perrito nuevo! ¡Qué emoción! Bienvenido a nuestra pequeña aldea.",
                    "Me llamo Maya. Soy la tejedora del pueblo. Hago bufandas, mantas... ¡y a veces disfraces para perros!"
                },
                generalLines: new[] {
                    "¿Has probado los pasteles de miel de la señora Rosa? ¡Son los mejores del reino!",
                    "Mi gato Michi y yo llevamos bien la convivencia. Aunque a veces le roba la cama al perro del vecino.",
                    "¿Ves esa bufanda morada que llevo? La teji con lana de oveja mágica. Dicen que da buena suerte.",
                    "Extraño los días en que la aldea estaba llena de viajeros. Desde que robaron los cristales, pocos se aventuran."
                },
                loreLines: new[] {
                    "Mi abuela me contaba que antes los cristales iluminaban la aldea por las noches. ¡No necesitábamos antorchas!",
                    "Esta aldea se llama 'Lumina'. Lo de siempre — le pusieron ese nombre por la luz de los cristales.",
                    "El tejido tradicional de aquí incorpora patrones basados en los cristales. ¿Ves los rombos azules en esta tela?"
                },
                tipLines: new[] {
                    "Si encuentras flores silvestres en el bosque, tráemelas. Las uso para tintes naturales.",
                    "Cuando llueva, busca refugio bajo los árboles grandes. Las raíces hacen de paraguas.",
                    "La señora Lena sabe mucho sobre hierbas medicinales. Si te lastimas, ve a verla."
                },
                dangerLines: new[] {
                    "¡Ay, por favor ten cuidado! Estos días hay criaturas extrañas rondando cerca del río.",
                    "Anoche escuché aullidos que no eran de ningún animal que conozco. Algo raro pasa en el bosque."
                },
                farewells: new[] {
                    "¡Cuídate mucho! Y si encuentras lana de colores especiales, ¡guárdame un poco!",
                    "¡Hasta pronto! Que tus patitas te lleven lejos y te traigan de vuelta sano."
                }
            );

            CreateDialogue("Aldeana_Rosa",
                npcName:    "Rosa",
                profession: "Panadera y Cocinera",
                nameColor:  new Color(1f, 0.6f, 0.6f),
                greetings: new[] {
                    "¡Cielos! ¡Un viajero! ¡Qué alegría! ¿Quieres probar mi pastel de manzana recién horneado?",
                    "Me llamo Rosa. Llevo 30 años horneando para este pueblo. ¡La harina es mi magia!"
                },
                generalLines: new[] {
                    "Hoy hice pan de nuez, rosquillas de miel y un estofado de zanahorias. ¡Todo para el pueblo!",
                    "¿Sabías que la canela tiene propiedades mágicas? Un poco en el pastel y hasta los espíritus sonríen.",
                    "El mercader de especias llegó ayer. Le compré azafrán del sur. ¡Caro pero vale cada moneda!",
                    "Mi receta de galletas con cristal molido... ¡ah, no! Eso era antes. Ya no tenemos cristales. Qué tristeza."
                },
                loreLines: new[] {
                    "Cuando yo era niña, en los días de festival molíamos un poco de cristal de luz en la harina. ¡El pan brillaba de noche!",
                    "Hay una planta en el bosque norte — el 'tomillo de luna' — que solo crece bajo la luz de los cristales.",
                    "Mi bisabuela era la cocinera del rey. Aprendí sus recetas secretas. Pero hay una que no puedo hacer sin cristales..."
                },
                tipLines: new[] {
                    "Si encuentras manzanas silvestres en el bosque, son perfectas para energía. ¡Come una antes de una batalla!",
                    "Las setas rojas con puntos blancos son venenosas. Las marrones con lunares amarillos, deliciosas.",
                    "El olor a pan fresco puede calmar a muchos animales del bosque. ¡Recuerda eso!"
                },
                dangerLines: new[] {
                    "Ayer vinieron dos exploradores pidiendo comida. Estaban asustados. Decían que algo los persiguió desde las ruinas.",
                    "Por favor, no te quedes en el bosque de noche. Las criaturas de la oscuridad son más fuertes sin la luz de los cristales."
                },
                farewells: new[] {
                    "¡Lleva estas galletas para el camino! (Te imaginas dándote una galleta invisible) ¡Buena suerte!",
                    "¡Vuelve cuando quieras! Siempre habrá algo caliente esperándote en mi cocina."
                }
            );

            CreateDialogue("Aldeana_Lena",
                npcName:    "Lena",
                profession: "Herbolaria y Curandera",
                nameColor:  new Color(0.6f, 1f, 0.7f),
                greetings: new[] {
                    "Shh... estaba escuchando a las plantas. Dicen cosas interesantes si sabes cómo escuchar.",
                    "¡Ah, un perro! Los perros tienen un sentido especial para la energía natural. Me alegra conocerte."
                },
                generalLines: new[] {
                    "Esta hoja de menta silvestre es buena para el dolor de estómago. Esta otra, para calmar los nervios.",
                    "Hay 47 plantas medicinales en el bosque cercano. Las conozco a todas por nombre.",
                    "¿Sientes ese aroma? Es lavanda. La planto alrededor de mi casa para mantener alejados los mosquitos... y los malos sueños.",
                    "Hoy el viento viene del norte. Eso significa lluvia mañana por la tarde. Las plantas me lo dicen."
                },
                loreLines: new[] {
                    "Los cristales de luz tenían propiedades curativas. Una astilla de cristal en el agua la purificaba al instante.",
                    "Hay una leyenda de una flor llamada 'Flor de Cristal' que solo nace donde un cristal ha estado. Nunca la he visto.",
                    "Los animales del bosque se acercan a mí porque respeto sus plantas. El respeto va y viene.",
                    "Mi maestra era una anciana ermitaña del bosque. Me enseñó que cada planta tiene un espíritu que puede ayudarte."
                },
                tipLines: new[] {
                    "Si te lastimas, busca hojas de plátano grandes. Las puedes usar como vendaje natural.",
                    "El musgo del lado norte de los árboles siempre apunta al norte. Úsalo como brújula en el bosque.",
                    "Antes de atacar a un enemigo, huélelo. El olor dice mucho sobre sus debilidades.",
                    "Si encuentras una fuente de agua cristalina, descansa junto a ella. Restaura la energía más rápido."
                },
                dangerLines: new[] {
                    "Mis plantas están inquietas. Eso solo pasa cuando hay magia oscura cerca. Ten mucho cuidado.",
                    "Traje algunas hierbas protectoras del bosque. Ojalá pueda proteger a toda la aldea con ellas."
                },
                farewells: new[] {
                    "¡Que la tierra te guíe y el viento te ayude! Regresa si necesitas curarte.",
                    "Ve con cuidado, amigo de cuatro patas. La naturaleza estará de tu lado."
                }
            );

            CreateDialogue("Guardia_Aldric",
                npcName:    "Guardia Aldric",
                profession: "Capitán de la Guardia",
                nameColor:  new Color(0.7f, 0.8f, 1f),
                greetings: new[] {
                    "¡Alto! Esta es la entrada principal de la Aldea Lumina. Identifícate.",
                    "...Un perro. Bien. Los perros son bienvenidos — son los mejores aliados en batalla.",
                    "Me llamo Aldric. Capitán de la Guardia. Llevo 15 años protegiendo esta aldea. Ahora sin cristales, es el doble de difícil."
                },
                generalLines: new[] {
                    "Última guardia fue tranquila. Aunque escuché movimientos en el bosque al amanecer.",
                    "La muralla norte necesita reparaciones. Pero sin cristales para fortalecer la piedra, tarda el doble.",
                    "Tengo seis guardias bajo mi mando. Buenos chicos. Pero ojalá tuviéramos más.",
                    "¿Has visto actividad sospechosa por el este? Hay reportes de sombras moviéndose contra el viento."
                },
                loreLines: new[] {
                    "Hace diez años, las murallas de esta aldea brillaban de noche gracias a los cristales incrustados en la piedra.",
                    "Los cristales no solo daban luz — repelían a las criaturas de oscuridad. Sin ellos, somos vulnerables.",
                    "Mi abuelo fue el último Gran Guardián de los Cristales. Cuando murió, yo era demasiado joven para entender lo que perdimos.",
                    "El Tratado de Lumina de hace 200 años estipula que los cristales deben mantenerse en la aldea. Alguien lo violó."
                },
                tipLines: new[] {
                    "En batalla: mantén siempre la espalda cubierta. Un buen perro guerrero nunca da la espalda al enemigo.",
                    "Si encuentras ruinas antiguas, busca los grabados en las paredes. Contienen información táctica de batallas pasadas.",
                    "La velocidad supera a la fuerza. Un golpe rápido y preciso vale más que diez lentos.",
                    "El portón oeste lleva al bosque principal. El camino norte, a las Ruinas. Elige sabiamente según tu nivel."
                },
                dangerLines: new[] {
                    "¡Alerta máxima! Reportaron criaturas desconocidas a 2 kilómetros al norte. ¡Mantente en guardia!",
                    "Dos de mis hombres no volvieron del patrullaje nocturno. Algo serio está pasando."
                },
                farewells: new[] {
                    "Que la espada sea rápida y el escudo sea firme. ¡Suerte, camarada!",
                    "¡Regresa victorioso! La aldea te necesita."
                }
            );

            CreateDialogue("Mago_Erwin",
                npcName:    "Mago Erwin",
                profession: "Estudioso de Cristales",
                nameColor:  new Color(0.85f, 0.6f, 1f),
                greetings: new[] {
                    "Hmm... interesante. Tus ondas áuricas son compatibles con la frecuencia cristalina. Fascinante.",
                    "¡Ah, un nuevo sujeto de estudio! Quiero decir... ¡un bienvenido visitante! Me llamo Erwin.",
                    "Llevo 40 años estudiando los cristales de luz. Y ahora que han desaparecido... tengo más preguntas que nunca."
                },
                generalLines: new[] {
                    "Los cristales vibran a 432 Hz — la frecuencia del universo. ¿Sabías eso? No. Claro que no. Nadie lo sabe excepto yo.",
                    "Estoy intentando crear un cristal sintético con polvo de estrella y lágrimas de ángel. Va... lento.",
                    "He escrito 23 tomos sobre cristalografía mágica. El tomo 24 está a medias. ¡Esto me da material para el capítulo 7!",
                    "El cristal rojo amplifica emociones. El azul, el pensamiento. El verde, la vitalidad. El amarillo... explosivo. No lo toques."
                },
                loreLines: new[] {
                    "Los cinco cristales no son objetos aleatorios. Fueron tallados de un único cristal primigenio hace 1000 años.",
                    "Cada cristal tiene una conciencia dormida. Si los tratas con respeto, te guiarán. Si los tratas mal... mejor no saberlo.",
                    "La ubicación de los cristales no es accidental. Fueron colocados en los cinco puntos de poder del mapa sagrado del reino.",
                    "Existe un ritual perdido llamado 'La Resonancia'. Si los cinco cristales se unen y vibran en armonía... el reino renace.",
                    "El ladrón de los cristales sabía exactamente dónde estaban. Solo alguien con acceso a los archivos secretos pudo saberlo."
                },
                tipLines: new[] {
                    "Acerca un cristal a una fuente de agua. Si el agua brilla, hay más cristales cerca.",
                    "Los cristales reaccionan al canto. Si cantas la nota DO grave cerca de donde crees que está, vibrará.",
                    "No mezcles cristales rojo y azul sin guantes de cuero dragón. Yo aprendí eso por las malas. (Señala sus cejas chamuscadas)",
                    "Hay un libro en las ruinas del norte — 'Guía de Campo para Cristales'. Consíguelo si puedes."
                },
                dangerLines: new[] {
                    "Mis instrumentos detectan perturbaciones en el campo cristalino. La oscuridad está más cerca de lo que pensamos.",
                    "¡No! ¡Mis cálculos eran correctos! Si la oscuridad llega a la aldea sin los cristales... las consecuencias serán catastróficas."
                },
                farewells: new[] {
                    "¡Ve y recupera esos cristales! ¡La ciencia te respalda! Y yo también... desde aquí, a salvo.",
                    "Toma nota de todo lo que veas. ¡Necesito datos para el tomo 24!"
                },
                hasQuest:    true,
                questOffer:  "¿Puedes traerme una muestra de tierra de las Ruinas del Norte? Creo que hay energía cristalina residual allí.",
                questActive: "¿Encontraste tierra de las Ruinas? Recoge la que tenga brillo azulado.",
                questComplete: "¡Extraordinario! Esta tierra tiene rastros de cristal puro. ¡Esto confirma mi Teoría de la Resonancia Fragmentada! Toma esta recompensa.",
                questCoins:  8
            );

            CreateDialogue("Lili_Nina",
                npcName:    "Lili",
                profession: "Niña de la Aldea",
                nameColor:  new Color(1f, 0.9f, 0.5f),
                greetings: new[] {
                    "¡¡¡PERRITOOOOO!!! ¡¡¡Eres tan bonito y esponjoso!!!",
                    "*te abraza con toda su fuerza*",
                    "¡Mi nombre es Lili! ¡Tengo 8 años y sé contar hasta 1000! ¿Cuántos años tienes tú?"
                },
                generalLines: new[] {
                    "¡Ayer vi una mariposa azul con alas de cristal! El abuelo dice que soy la única que las ve. Creo que es verdad.",
                    "¿Sabes cuál es mi animal favorito? ¡Los perros! ¡Pero también los gatos! ¡Y los conejos! ¡Y los dragones!",
                    "Estoy practicando para ser maga. Ya sé hacer aparecer flores de la nada. ¿Quieres ver? *saca una flor del bolsillo*",
                    "Mi abuela me dijo que cuando yo era bebé, dormía con un cristal de luz en la cuna. Me hacía soñar cosas bonitas."
                },
                loreLines: new[] {
                    "El abuelo Eldor me contó que los cristales cantan cuando nadie los escucha. Yo creo que sí, porque a veces escucho música de ningún lado.",
                    "Hay un árbol en el bosque que tiene el tronco brillante. ¡Mi papá dice que no debo ir, pero yo ya fui! ¡Shh!",
                    "Dicen que hace mucho tiempo había hadas de luz que vivían dentro de los cristales. ¿Crees que sea verdad?"
                },
                tipLines: new[] {
                    "¡Si estás triste, cuenta estrellas! Yo cuento hasta 47 antes de quedarme dormida.",
                    "Las margaritas traen buena suerte. ¡Lleva siempre una en la oreja!",
                    "Si te pierdes en el bosque, sigue el río. Siempre lleva a algún lugar."
                },
                dangerLines: new[] {
                    "Anoche soñé con sombras grandes. Mi mamá dice que son solo sueños pero... se sentían muy reales.",
                    "¡Por favor regresa! Si te pasa algo me pondré muy triste."
                },
                farewells: new[] {
                    "¡Vuelve pronto! ¡Te espero aquí con flores!",
                    "¡Cuídate muchoooo! ¡Y si encuentras piedras brillantes, tráeme una de recuerdo!"
                }
            );

            CreateDialogue("Granjero_Tomas",
                npcName:    "Granjero Tomás",
                profession: "Agricultor",
                nameColor:  new Color(0.8f, 1f, 0.6f),
                greetings: new[] {
                    "¡Hola, forastero! ¿Vienes a comprar verduras? Las de esta temporada son excepcionales.",
                    "¡Ey, perrito! ¿También vienes a cavar en mi jardín? Tengo suficiente con el topo del norte..."
                },
                generalLines: new[] {
                    "Este año la cosecha de zanahorias es la mejor en décadas. ¡Las plagas no se atrevieron!",
                    "Mi buey Pancho cumple 12 años esta semana. Le hice un pastel de heno. Le encantó.",
                    "Si quieres trabajar duro para comer bien, la tierra no miente. Todo lo que siembras, lo cosechas.",
                    "Cuarenta años arando esta tierra. Conozco cada piedra, cada surco, cada gusano."
                },
                loreLines: new[] {
                    "Mis abuelos plantaban con la luz de los cristales de guía. Las cosechas eran el doble de grandes.",
                    "Hay una parte del campo donde nada crece bien. Desde que los cristales desaparecieron, se puso peor.",
                    "La tierra tiene memoria. Sabe cuándo la cuidas bien y cuándo la descuidas. Los cristales eran su vitamina."
                },
                tipLines: new[] {
                    "Las zanahorias dan buena visión nocturna. ¡Come una antes de entrar a cuevas oscuras!",
                    "Si ves un campo de girasoles, los que apuntan al sol siempre te orientan.",
                    "La lluvia de primavera trae lombrices a la superficie. Las aves las siguen. ¡Las aves conocen todos los caminos!"
                },
                dangerLines: new[] {
                    "Algo se está comiendo mis cultivos de noche. No son conejos — los rastros son demasiado grandes.",
                    "Mis animales están nerviosos. Cuando el ganado no come, es que algo va mal en la naturaleza."
                },
                farewells: new[] {
                    "¡Buen viaje! Y si encuentras buena tierra negra por ahí, ¡avísame!",
                    "Regresa cuando quieras. Aquí siempre hay trabajo y siempre hay comida."
                }
            );

            CreateDialogue("Mercader_Bruno",
                npcName:    "Mercader Bruno",
                profession: "Comerciante Ambulante",
                nameColor:  new Color(1f, 0.8f, 0.3f),
                greetings: new[] {
                    "¡Bienvenido, bienvenido! ¡Bruno's Emporium, el mejor comercio del reino!",
                    "¿Buscas equipo? ¿Provisiones? ¿Mapas? ¡Todo lo tengo, todo lo consigo, todo tiene precio!"
                },
                generalLines: new[] {
                    "Acabo de llegar del Puerto del Sur. Los mercados de allá están convulsos — todos hablan de los cristales perdidos.",
                    "Tengo mapas de las ruinas del norte, del bosque eterno y de la cueva de cristal. ¿Te interesa alguno?",
                    "El precio del cristal molido se disparó en el mercado negro. Gente sin escrúpulos, te lo digo.",
                    "Soy el comerciante más honesto del reino. ¡Solo subo los precios cuando la demanda lo justifica!"
                },
                loreLines: new[] {
                    "Yo viajé por todo el reino. Los cristales de luz no solo existen aquí — hay fragmentos en todos lados, si sabes dónde buscar.",
                    "El Gremio de Mercaderes tiene un archivo secreto con la ubicación de todos los cristales conocidos. No es fácil acceder a él.",
                    "Dicen que el ladrón de los cristales los está vendiendo al mejor postor en las ciudades del norte. ¡Qué infame!"
                },
                tipLines: new[] {
                    "Consejo de viajero: lleva siempre monedas de distintos materiales. En algunas regiones, el oro no vale nada pero el cobre sí.",
                    "Un buen mapa vale más que diez espadas. ¡La información es poder!",
                    "En el mercado de Eastgate puedes encontrar provisiones a mitad de precio si vas al amanecer."
                },
                dangerLines: new[] {
                    "El camino del norte está bloqueado por criaturas extrañas. Tuve que desviarme tres días. ¡Tres días!",
                    "Escuché en una posada que hay un ejército de sombras moviéndose hacia el reino. No sé si creerlo... pero por si acaso, tengo escudos en oferta."
                },
                farewells: new[] {
                    "¡Vuelve cuando necesites algo! ¡Bruno siempre tiene lo que buscas... a precio justo!",
                    "¡Que el viento llene tus velas y tu bolsa de monedas, amigo!"
                }
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Diálogos Generados",
                "✅ Diálogos completos creados para:\n\n" +
                "• Sabio Eldor (questgiver principal)\n" +
                "• Maya (tejedora)\n" +
                "• Rosa (panadera)\n" +
                "• Lena (herbolaria)\n" +
                "• Guardia Aldric (capitán)\n" +
                "• Mago Erwin (estudioso, quest secundaria)\n" +
                "• Lili (niña)\n" +
                "• Granjero Tomás\n" +
                "• Mercader Bruno\n\n" +
                $"Guardados en: {DIALOGUE_FOLDER}/",
                "¡Perfecto!");
        }

        // ── Helper para crear un ScriptableObject ────────────────────────────
        private static void CreateDialogue(string fileName, string npcName, string profession,
            Color nameColor, string[] greetings, string[] generalLines, string[] loreLines,
            string[] tipLines, string[] dangerLines, string[] farewells,
            bool hasQuest = false, string questOffer = "", string questActive = "",
            string questComplete = "", int questCoins = 0)
        {
            string path = $"{DIALOGUE_FOLDER}/{fileName}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<Platformer.TopDown.NPCDialogueData>(path);
            Platformer.TopDown.NPCDialogueData data;

            if (existing != null)
            {
                data = existing;
            }
            else
            {
                data = ScriptableObject.CreateInstance<Platformer.TopDown.NPCDialogueData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.npcName        = npcName;
            data.profession     = profession;
            data.nameColor      = nameColor;
            data.greetings      = greetings;
            data.generalLines   = generalLines;
            data.loreLines      = loreLines;
            data.tipLines       = tipLines;
            data.dangerLines    = dangerLines;
            data.farewells      = farewells;
            data.hasQuest       = hasQuest;
            data.questOffer     = questOffer;
            data.questActiveReminder = questActive;
            data.questComplete  = questComplete;
            data.questRewardCoins = questCoins;

            EditorUtility.SetDirty(data);
        }
    }
}
