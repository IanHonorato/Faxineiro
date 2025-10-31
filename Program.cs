using System;
using System.Drawing;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Excluir fotos com resolução abaixo do limite ===");

        Console.Write("Informe o diretório raiz: ");
        string directoryPath = Console.ReadLine()!;

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("❌ Diretório não encontrado!");
            return;
        }

        Console.Write("Informe a resolução mínima (ex: 80x80): ");
        string resolutionInput = Console.ReadLine()!;
        var parts = resolutionInput.Split('x', 'X');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int minWidth) ||
            !int.TryParse(parts[1], out int minHeight))
        {
            Console.WriteLine("❌ Formato inválido. Use algo como 80x80.");
            return;
        }

        Console.WriteLine($"\n🔍 Buscando imagens com resolução abaixo de {minWidth}x{minHeight} em {directoryPath}...\n");

        string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories); // 🔹 Agora inclui subpastas

        int deleted = 0;
        int corrompidas = 0;

        foreach (var file in files)
        {
            try
            {
                if (!extensions.Contains(Path.GetExtension(file).ToLower()))
                    continue;

                // Lê os bytes sem travar o arquivo
                byte[] bytes = File.ReadAllBytes(file);
                int width, height;

                // Obtém dimensões — e ignora arquivos corrompidos
                try
                {
                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        width = img.Width;
                        height = img.Height;
                    }
                }
                catch (OutOfMemoryException)
                {
                    corrompidas++;
                    Console.WriteLine($"🚫 Arquivo corrompido: {Path.GetFileName(file)}");
                    continue;
                }
                catch (ArgumentException)
                {
                    corrompidas++;
                    Console.WriteLine($"🚫 Arquivo inválido: {Path.GetFileName(file)}");
                    continue;
                }

                // 🔹 Exclui se for menor que a resolução mínima
                if (width <= minWidth && height <= minHeight)
                {
                    try
                    {
                        File.Delete(file);
                        deleted++;
                        Console.WriteLine($"🗑️  Excluído: {file} ({width}x{height})");
                    }
                    catch (IOException ioEx)
                    {
                        Console.WriteLine($"⚠️ Erro ao excluir {Path.GetFileName(file)}: {ioEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro inesperado em {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        Console.WriteLine($"\n✅ Concluído!");
        Console.WriteLine($"   Total excluídas: {deleted}");
        Console.WriteLine($"   Arquivos corrompidos ignorados: {corrompidas}");
    }
}
