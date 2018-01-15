using System;
using System.Linq;

namespace CG
{
    public class BaseStateReader
    {
        protected string lastLine;
        protected bool logToError = true;
        protected Func<string> readLine;

        public BaseStateReader(string input)
        {
            var lines = input.Split('|');
            var index = 0;
            logToError = false;
            readLine = () => index < lines.Length ? lines[index++] : null;
        }

        public BaseStateReader(Func<string> consoleReadLine)
        {
            readLine = () =>
            {
                lastLine = consoleReadLine();
                if (logToError)
                    Console.Error.Write(lastLine + "|");
                return lastLine;
            };
        }

        public int ReadInt()
        {
            return int.Parse(readLine());
        }

        public int[] ReadInts()
        {
            return readLine().Split().Select(int.Parse).ToArray();
        }

        public Vec ReadVec()
        {
            return Vec.Parse(readLine());
        }
    }
}
