using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordAnalyzer
{
    internal class PasswordCracker
    {
        /// <summary>
        /// 测试集密码字典，每个密码被猜中多少次
        /// </summary>
        private readonly Dictionary<string, int> hitPwdDict = new();
        /// <summary>
        /// 读取的代码文件有多少行
        /// </summary>
        public int PasswordLine { get; private set; } = 0;
        /// <summary>
        /// 忽略了多少行密码
        /// </summary>
        public int IgnoredLine { get; private set; } = 0;
        /// <summary>
        /// 有效的待猜测密码有多少行
        /// </summary>
        public int PasswordCount
        {
            get
            {
                return hitPwdDict.Count;
            }
        }

        /// <summary>
        /// 共尝试了多少次密码
        /// </summary>
        public ulong PasswordTried { get; private set; } = 0;
        /// <summary>
        /// 共命中了多少次密码（包含重复项）
        /// </summary>
        public int HitedPasswordCount { get; private set; } = 0;

        private Thread thread = null!;
        private NamedPipeServerStream pipe = null!;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// 加载测试集密码文件
        /// </summary>
        /// <param name="path"></param>
        public bool LoadFile(string path)
        {
            StreamReader sr;
            try
            {
                sr = File.OpenText(path);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"打开文件时出现错误：{ex.Message}");
                return false;
            }

            Console.WriteLine("文件加载中，请稍后...");
            string line;
            while ((line = sr.ReadLine()!) != null)
            {
                if (line == string.Empty || !hitPwdDict.TryAdd(line, 0))
                {
                    //空行或者重复密码
                    ++IgnoredLine;
                }
                else
                {

                }
                ++PasswordLine;
            }

            sr.Close();
            Console.WriteLine($"加载完毕。测试集密码共{PasswordLine}行，其中{IgnoredLine}行已被忽略。");
            return true;
        }

        /// <summary>
        /// 建立管道并准备从管道中读取数据
        /// </summary>
        public void OpenNamePipeLineReadThread()
        {
            pipe = new NamedPipeServerStream("pwdcracker");
            Console.WriteLine("请将预测密码的输出重定向到\\\\.\\pipe\\pwdcracker命名管道中");

            thread = new(PipeLineReadThread!);
            thread.Start(cts.Token);
        }

        /// <summary>
        /// 管道读取线程，负责从管道中读取数据并做分析
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async void PipeLineReadThread(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            using StreamReader reader = new(pipe);
            try
            {
                await pipe.WaitForConnectionAsync().WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"等待管道被连接时发生错误：{ex.Message}");
                return;
            }

            while (!token.IsCancellationRequested && pipe.IsConnected)
            {
                string? line = reader.ReadLine();
                if (line == null) continue;

                if (hitPwdDict.ContainsKey(line))
                {
                    ++hitPwdDict[line];
                    ++HitedPasswordCount;
                }
                else
                {

                }
                ++PasswordTried;
            }

        }

        /// <summary>
        /// 停止管道读取线程
        /// </summary>
        public void StopThread()
        {
            cts.Cancel();
            thread.Join();
        }

        /// <summary>
        /// 输出当点状态
        /// </summary>
        public void PrintStatus()
        {
            if (pipe.IsConnected)
            {
                string i = "<没有程序>";
                try
                {
                    i = pipe.GetImpersonationUserName();
                }
                catch
                {

                }
                Console.WriteLine();
                Console.WriteLine($"程序{pipe.GetImpersonationUserName()}");
                Console.WriteLine($"已处理密码{PasswordTried}个\t{HitedPasswordCount}条密码命中测试集");
            }
            else
            {
                Console.WriteLine("管道未连接");
            }
        }
        ~PasswordCracker()
        {
            StopThread();
            pipe.Close();
        }
    }
}
