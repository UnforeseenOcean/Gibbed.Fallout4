﻿/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Gibbed.Fallout4.FileFormats;
using NDesk.Options;

namespace Gibbed.Fallout4.Unpack
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            string filterPattern = null;
            bool overwriteFiles = false;
            bool verbose = false;

            var options = new OptionSet()
            {
                { "o|overwrite", "overwrite existing files", v => overwriteFiles = v != null },
                { "f|filter=", "only extract files using pattern", v => filterPattern = v },
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_ba2 [output_dir]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = Path.GetFullPath(extras[0]);
            string outputPath = extras.Count > 1
                                    ? Path.GetFullPath(extras[1])
                                    : Path.ChangeExtension(inputPath, null) + "_unpack";

            Regex filter = null;
            if (string.IsNullOrEmpty(filterPattern) == false)
            {
                filter = new Regex(filterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            using (var input = File.OpenRead(inputPath))
            {
                var basePosition = input.Position;
                var type = ArchiveFile.ReadType(input);
                input.Position = basePosition;

                switch (type)
                {
                    case ArchiveType.General:
                    {
                        UnpackGeneral.Program.Unpack(input, filter, outputPath, overwriteFiles, verbose);
                        break;
                    }

                    case ArchiveType.TextureDX10:
                    {
                        UnpackTexture.Program.Unpack(input, filter, outputPath, overwriteFiles, verbose);
                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }
    }
}
