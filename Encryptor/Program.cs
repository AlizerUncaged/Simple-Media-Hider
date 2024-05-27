using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.Write("Enter file full of shit: ");
        string er = Console.ReadLine().Replace("\"", "");

        if (string.IsNullOrWhiteSpace(er))
            er = @"C:\Users\Gumayusi\Desktop\carn.txt";

        Console.Write("Encrypt or Decrypt? (e/d): ");

        string choice = Console.ReadLine();
        var fi = 0;
        var shits = File.ReadAllLines(er);

        foreach (var dirPath in shits)
        {
            var currentFi = 0;

            string[] mediaExtensions =
                { ".jpeg", ".bmp", ".jpg", ".png", ".gif", ".webp", ".mp4", ".mov", ".jfif", ".ico" };


            foreach (var filePath in Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(filePath);

                if (choice.ToLower() == "e" && mediaExtensions.Contains(ext.ToLower()) && !ext.Contains("-")) // Encrypt
                {
                    currentFi++;
                    fi++;
                    string newFileName = Path.GetFileNameWithoutExtension(filePath) + "." +
                                         new string(ext.Reverse().ToArray()).Replace(".", "") +
                                         "-";

                    string newPath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);
                    int tries = 0;

                    do
                    {
                        if (!File.Exists(newPath))
                            File.Move(filePath, newPath);

                        newFileName = Path.GetFileNameWithoutExtension(filePath) + $"-{tries}." +
                                      new string(ext.Reverse().ToArray()).Replace(".", "") +
                                      "-";

                        newPath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

                        tries++;
                    } while (File.Exists(newPath));
                }
                else if (choice.ToLower() == "d" && ext.Contains("-")) // Decrypt
                {
                    currentFi++;
                    fi++;

                    var reversedExt = new string(ext.TrimEnd('-').Trim('.').Reverse().ToArray());

                    if (mediaExtensions.Any(x => reversedExt.Contains(x.Trim('.'))))
                    {
                        string newFileName = Path.GetFileNameWithoutExtension(filePath) + "." + reversedExt;
                        string newPath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

                        if (File.Exists(newPath))
                            Console.WriteLine($"File already exists: {newPath}");
                        if (!File.Exists(newPath))
                            File.Move(filePath, newPath);
                        else if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                }
            }

            Console.WriteLine($"we're good, processed {currentFi} files for {dirPath}");
        }

        Console.WriteLine($"we're good, processed {fi} files");

        Console.WriteLine(
            $"we done for {(choice.Trim().Equals("e", StringComparison.InvariantCultureIgnoreCase) ? "encrypt" : "decrypt")}");
    }
}