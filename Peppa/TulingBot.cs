using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peppa
{
    public class TulingBot
    {
        public static List<string> Bot = new List<string>
        {
            "a66de819c0b44aa598e69ce6a7a9e3ed",
            "9a4792ca7503438b86c199cc3c754d12",
            "0031f41b8b14450f98c67aac9d88250c",
            "b7366e88784147548e89db1886f7ca57",
            "49084d615e0b439c91680e2f5e9d9bf7"
        };

        public static HashSet<string> ExceedLimit = new HashSet<string>();

        public static List<int> Error = new List<int>
        {
            5000,
            6000,
            4000,
            4001,
            4002,
            4003,
            4005,
            4007,
            4100,
            4200,
            4300,
            4400,
            4500,
            4600,
            4602,
            7002,
            8008,
        };
    }
}
