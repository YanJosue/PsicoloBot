using Syn.Bot.Siml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FuzzySharp;
using Syn.Bot.Oscova; // <-- 1. Añadimos la biblioteca para comparación difusa

public class TerminoEmocional
{
    public required string Emocion { get; set; }
    public required string Termino { get; set; }
}

public class PsicoloboBot
{
    private readonly OscovaBot Bot;
    private List<TerminoEmocional> BaseDeConocimientos;

    public PsicoloboBot()
    {
        Bot = new OscovaBot();
        BaseDeConocimientos = new List<TerminoEmocional>();
    }

    // --- 2. IMPLEMENTACIÓN DEL CEREBRO DEL BOT ---
    public string DetectarEmocion(string textoUsuario)
    {
        Console.WriteLine("\n--- Iniciando Detección de Emoción ---");
        var conteoEmociones = new Dictionary<string, int>();

        // Dividimos la entrada del usuario en palabras individuales.
        var palabrasUsuario = textoUsuario.ToLower().Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var palabra in palabrasUsuario)
        {
            // Buscamos la mejor coincidencia para cada palabra en nuestra base de conocimientos.
            foreach (var entradaConocimiento in BaseDeConocimientos)
            {
                // Usamos FuzzySharp para obtener un porcentaje de similitud.
                int similitud = Fuzz.Ratio(palabra, entradaConocimiento.Termino.ToLower());

                // Si la similitud es alta (ej. > 85%), contamos la emoción.
                if (similitud > 85)
                {
                    Console.WriteLine($"  -> Coincidencia encontrada: '{palabra}' se parece a '{entradaConocimiento.Termino}' (Emoción: {entradaConocimiento.Emocion}, Similitud: {similitud}%)");

                    if (!conteoEmociones.ContainsKey(entradaConocimiento.Emocion))
                    {
                        conteoEmociones[entradaConocimiento.Emocion] = 0;
                    }
                    conteoEmociones[entradaConocimiento.Emocion]++;

                    // Pasamos a la siguiente palabra del usuario para no contar una palabra dos veces.
                    break;
                }
            }
        }

        if (conteoEmociones.Count == 0)
        {
            Console.WriteLine("--- No se detectaron emociones claras. ---");
            return "NO_DETECTADA";
        }

        // Buscamos la emoción con el conteo más alto.
        var emocionDominante = conteoEmociones.OrderByDescending(kv => kv.Value).FirstOrDefault();
        Console.WriteLine($"--- Detección Finalizada. Emoción dominante: {emocionDominante.Key} (Conteo: {emocionDominante.Value}) ---\n");

        return emocionDominante.Key;
    }

    // --- Carga y Entrenamiento ---
    public void CargarBaseDeConocimientos(string rutaArchivo)
    {
        Console.WriteLine("Cargando base de conocimientos...");
        try
        {
            var lineas = File.ReadAllLines(rutaArchivo).Skip(1);
            foreach (var linea in lineas)
            {
                var partes = linea.Split(',');
                if (partes.Length >= 2)
                {
                    BaseDeConocimientos.Add(new TerminoEmocional
                    {
                        Emocion = partes[0].Trim(),
                        Termino = partes[1].Trim()
                    });
                }
            }
            Console.WriteLine($"-> ¡Carga exitosa! Se cargaron {BaseDeConocimientos.Count} términos.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR al cargar la base de conocimientos: {ex.Message}");
        }
    }

    // --- Corrección del error CS1061 ---
    // El método "Train" no existe en la clase OscovaBot según las firmas proporcionadas.
    // Para solucionar este problema, reemplazamos la línea `Bot.Train();` con el método adecuado para inicializar el bot.

    public void Entrenar()
    {
        Console.WriteLine("Inicializando bot...");
        try
        {
            // Si tienes un archivo workspace.oscova, usa ImportWorkspace.
            // Si NO tienes, simplemente elimina la línea ImportWorkspace y usa Train() si está disponible.
            // Si no tienes intenciones personalizadas, puedes inicializar el bot sin cargar workspace.
            // Si Train() no existe, simplemente deja el método vacío o muestra un mensaje.
            // Ejemplo seguro:
            // Bot.Train(); // Descomenta si existe este método en tu versión de Oscova
            Console.WriteLine("-> Bot listo (sin intenciones personalizadas).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR al inicializar el bot: {ex.Message}");
        }
    }

    // Reemplazo de la línea problemática para corregir el error CS1061
    public void IniciarChat()
    {
        Console.WriteLine("\n¡Hola! Soy Psicolobo. Escribe 'salir' para terminar.");
        while (true)
        {
            Console.Write("Tú: ");
            string? textoUsuario = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(textoUsuario)) continue;
            if (textoUsuario.ToLower() == "salir") break;

            // Intentamos evaluar solo si el bot está entrenado
            try
            {
                var resultado = Bot.Evaluate(textoUsuario);

                if (resultado.Intents.Count == 0)
                {
                    string emocionDetectada = DetectarEmocion(textoUsuario);
                    if (emocionDetectada != "NO_DETECTADA")
                    {
                        Console.WriteLine($"Bot: Percibo que podrías estar sintiendo algo como '{emocionDetectada}'. ¿Quieres hablar de eso?");
                    }
                    else
                    {
                        Console.WriteLine("Bot: No estoy seguro de entender completamente. ¿Puedes decírmelo de otra forma?");
                    }
                }
                else
                {
                    var intent = resultado.Intents.FirstOrDefault();
                    Console.WriteLine($"Bot: {intent?.ExpressionName ?? "No tengo una respuesta para esto."}");
                }
            }
            catch (InvalidOperationException)
            {
                // Si el bot no está entrenado, solo usamos el motor de emociones
                string emocionDetectada = DetectarEmocion(textoUsuario);
                if (emocionDetectada != "NO_DETECTADA")
                {
                    Console.WriteLine($"Bot: Percibo que podrías estar sintiendo algo como '{emocionDetectada}'. ¿Quieres hablar de eso?");
                }
                else
                {
                    Console.WriteLine("Bot: No estoy seguro de entender completamente. ¿Puedes decírmelo de otra forma?");
                }
            }
        }
    }
}

// --- Punto de Entrada de la Aplicación ---
public class Program
{
    public static void Main(string[] args)
    {
        var miBot = new PsicoloboBot();

        miBot.CargarBaseDeConocimientos("base_conocimientos.csv");
        miBot.Entrenar();
        miBot.IniciarChat();
    }
}
