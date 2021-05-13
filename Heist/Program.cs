using Heist.Extensions;
using Heist.Models;
using Nysc.API.Extensions;
using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Heist
{
    class Program
    {
        static void Main(string[] args)
        {
            string urlTemplate = "";
            int start = -1, end = int.MaxValue, index = -1;

            if (args.ContainsAny(out index, "--url", "-u") || args.Length > 0)
                urlTemplate = args.ElementAtOrDefault(index + 1);

            if (args.ContainsAny(out index, "--start", "-s"))
            {
                string arg = args.ElementAtOrDefault(index + 1);
                if (!int.TryParse(arg, out start)) DisplayHelp();
            }

            if (args.ContainsAny(out index, "--end", "-e"))
            {
                string arg = args.ElementAtOrDefault(index + 1);
                if (!int.TryParse(arg, out end)) DisplayHelp();
            }

            if (string.IsNullOrWhiteSpace(urlTemplate) || start < 0 || end < 0)
            {
                DisplayHelp();
                return;
            }

            HeistContext context = new HeistContext
            {
                Url = urlTemplate,
                Start = start,
                End = end
            };

            Console.WriteLine($"Downloading from: {urlTemplate}");
            Console.WriteLine($"From: {start} to {end}");
            Console.WriteLine("Starting in 2 seconds");
            
            Thread.Sleep(2000);
            StartDownload(context).Wait();
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Heist batch downloads file from a websites based on the template it is given");
            Console.WriteLine("USAGE: ");
            Console.WriteLine("heist.exe https://example.com/files/download/##num##.png -s 1 -e 100");
            Console.WriteLine("heist.exe --start 59 -end 90 --url https://anotherexample.com/pages/##num##.html ");
        }

        static async Task StartDownload(HeistContext context)
        {
            Uri uri = new Uri(context.Url);
            RestClient client = new RestClient();

            for (int i = context.Start; i <= context.End; i++)
            {
                string req = context.Url.BindTo(new { num = i }, "####");

                await Executor.CautiouslyExecuteAsync(async () =>
                {
                    Console.WriteLine($"Downloading: {req}");

                    var res = await client.ExecuteAsync(new RestRequest(req, Method.GET));
                    string fileName;

                    try
                    {
                        // Thanks to https://stackoverflow.com/a/59836722/8058709
                        // for pointing the way
                        var headervalue = res.Headers.FirstOrDefault(x => x.Name == "Content-Disposition")?.Value ?? "";
                        ContentDisposition contentDisposition = new ContentDisposition(Convert.ToString(headervalue));
                        fileName = contentDisposition.FileName;
                    }
                    catch
                    {
                        fileName = Path.GetFileName(req);
                    }


                    string path = Path.Combine(uri.Host, fileName);
                    string dir = Path.GetDirectoryName(path);

                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    await File.WriteAllBytesAsync(path, res.RawBytes);
                    string absolutePath = Path.GetFullPath(path);

                    Console.WriteLine($"Sucessfully downloaded {res.RawBytes.Length} bytes => ({absolutePath})");

                }, null, TimeSpan.FromSeconds(3), 3, (att, ex) =>
                {
                    Console.WriteLine(ex);
                    Console.WriteLine($"Failed to download ({req}) after {att} attempt(s)");

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();

                    if (att != 3)
                        Console.WriteLine("Retrying in 3 seconds");
                });
            }
        }
    }
}
