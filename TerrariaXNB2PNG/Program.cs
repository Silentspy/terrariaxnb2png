using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Management;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WinFormsGraphicsDevice;
using System.Collections.Generic;

namespace TerrariaXNB2PNG
{
    class Program
    {
        List<string> filesToDelete = new List<string>();

        [STAThread]
        public static void Main(string[] args)
        {
            if ((args.Length > 0) && File.Exists(args[0]))
            {
                new Program().instance(args);
            }
            else
            {
                Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " <path...>");
            }

            if (getParentProcess().ToLower().Equals("explorer") || getParentProcess().ToLower().Equals("devenv"))
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        public void instance(string[] args)
        {
            try
            {

                Form form = new Form();
                GraphicsDeviceService gds = GraphicsDeviceService.AddRef(form.Handle, form.ClientSize.Width, form.ClientSize.Height);
                ServiceContainer services = new ServiceContainer();
                services.AddService<IGraphicsDeviceService>(gds);
                var content = new ContentManager(services);

                foreach (string p in args)
                {
                    Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + p);
                    if (File.Exists(p))
                    {
                        if (Path.GetExtension(p).Equals(".xnb"))
                        {
                            ConvertToPng(content, p);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid file path or file");
                    }
                }

                foreach (string f in filesToDelete)
                {
                    File.Delete(f);
                }
                content.Unload();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static string getParentProcess()
        {
            var myId = Process.GetCurrentProcess().Id;
            var query = string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", myId);
            var search = new ManagementObjectSearcher("root\\CIMV2", query);
            var results = search.Get().GetEnumerator();
            results.MoveNext();
            var queryObj = results.Current;
            var parentId = (uint)queryObj["ParentProcessId"];
            var parent = Process.GetProcessById((int)parentId);
            return parent.ProcessName;
        }

        void ConvertToPng(ContentManager content, string filename)
        {
            try
            {
                Random rnd = new Random();
                string assetName = Path.GetFileName(filename.Replace(".xnb", ".temp.") + rnd.Next(1000 + 1) + ".xnb");
                File.Copy(filename, AppDomain.CurrentDomain.BaseDirectory + assetName);
                Texture2D tex = content.Load<Texture2D>(assetName.Replace(".xnb",""));
                string filenamePNGEXT = filename.Replace(".xnb", ".png");
                filesToDelete.Add(AppDomain.CurrentDomain.BaseDirectory + assetName);

                if (File.Exists(filenamePNGEXT))
                {
                    DialogResult result = MessageBox.Show(filenamePNGEXT + " Already exists, replace it?", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        using (FileStream file = File.Create(filenamePNGEXT))
                        {
                            tex.SaveAsPng(file, tex.Width, tex.Height);
                            Console.WriteLine("Created: " + file.Name);
                        }
                    }
                }
                else
                {
                    using (FileStream file = File.Create(filenamePNGEXT))
                    {
                        tex.SaveAsPng(file, tex.Width, tex.Height);
                        Console.WriteLine("Created: " + file.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void ConvertToNXB()
        {

        }
    }
}
