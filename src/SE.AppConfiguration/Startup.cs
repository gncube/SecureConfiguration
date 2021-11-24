using CommandLine;
using NLog;
using System;
using System.Configuration;

namespace SE.AppConfiguration
{
    public class Startup
    {
        readonly Logger log = LogManager.GetCurrentClassLogger();
        public int RunParser(string[] args)
        {
            // check the logger is configured.
            if (!log.IsEnabled(LogLevel.Info))
            {
                var clr = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING NLog configuration not found.");
                Console.WriteLine("Is NLog.config file missing ?");
                Console.WriteLine("A Console logger must be configured with atleast INFO level to see program output.");
            }

            NestedDiagnosticsLogicalContext.Push("Parsing");

            if (log.IsDebugEnabled)
            {
                foreach (var a in args)
                {
                    log.Debug($"Parameter: [{a}]");
                }
            }

            var exitcode = -1;
            try
            {
                var parser = new Parser(with =>
                {
                    with.CaseSensitive = false;
                    with.HelpWriter = Console.Out;
                });

                //exitcode = parser
                //    .ParseArguments<OptionsAppConfigSecurtiyScan, OptionsSecureApp, OptionsUnsecure, OptionsExport, OptionsImport, OptionsDeleteKey>(args)
                //        .MapResult(
                //        (OptionsAppConfigSecurtiyScan opts) => RunTask(opts),
                //        (OptionsSecureApp opts) => RunTask(opts),
                //        (OptionsUnsecure opts) => RunTask(opts),
                //        (OptionsExport opts) => RunTask(opts),
                //        (OptionsImport opts) => RunTask(opts),
                //        (OptionsDeleteKey opts) => RunTask(opts),
                //        (parserErrors) => -1 // Parser Error
                //        );
            }
            catch (Exception ex)
            {
                log.Fatal(ex, "Failed to parse commandline");
                exitcode = -255;
            }

            WaitKey();

            return exitcode;
        }

        private int RunTask(ProgramOptions options)
        {
            var optionTypeName = options.GetType().Name;

            NestedDiagnosticsLogicalContext.Push(optionTypeName);

            log.Debug($"Running [{optionTypeName}] ...");

            if (!options.Init())
            {
                log.Error("Failed to Initialise Option .");
                return -2;
            }

            try
            {
                NestedDiagnosticsLogicalContext.Push(optionTypeName + " execute");
                log.Debug("Starting execution.");
                options.Execute();
                NestedDiagnosticsLogicalContext.Pop();
                log.Debug("Task Complete.");
                return 0;
            }
            catch (Exception e)
            {
                log.Fatal(e, "Execution Failed.");
                return -2;
            }
        }

        /// <summary>
        /// Optionally wait on a key on exit (based on the 'WaitForKeyOnExit' app setting)
        /// Used for debug 
        /// </summary>
        private void WaitKey()
        {
            var val = ConfigurationManager.AppSettings["WaitForKeyOnExit"];

            if (string.IsNullOrWhiteSpace(val))
            {
                return;
            }

            bool op;
            if (bool.TryParse(val, out op))
            {
                if (op)
                {
                    Console.WriteLine("Please press a key to exit.");
                    Console.WriteLine("( to disable the required key press on exit, set app.config setting [WaitForKeyOnExit] = false )");
                    Console.ReadKey();
                }
            }
        }
    }
}
