
namespace PasswordAnalyzer
{

    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length !=2 || args[0]!="--file")
            {
                PrintHelp();
                return;
            }

            PasswordCracker passwordCracker = new PasswordCracker();
            if(!passwordCracker.LoadFile(args[1]))
            {
                return;
            }
            passwordCracker.OpenNamePipeLineReadThread();
            Console.WriteLine("按任意键打印当前状态信息，输入EOF（Windows下Ctrl+z）退出");
            Console.WriteLine("正在监视控制台输入...");

            while (true)
            {
                string read = Console.ReadLine()!;
                if (read == null)
                {
                    break;
                }
                else
                {
                    passwordCracker.PrintStatus();
                }
                Thread.Sleep(100);
            }

            passwordCracker.StopThread();
        }
        static void PrintHelp()
        {
            Console.WriteLine("程序语法：PasswordAnalyzer.exe --file <filepath>");
            Console.WriteLine("<filepath>：测试集密码的位置，utf-8文本文件，每条密码一行");
        }

        static void PrintBanner()
        {
            Console.WriteLine("");
        }
    }
}