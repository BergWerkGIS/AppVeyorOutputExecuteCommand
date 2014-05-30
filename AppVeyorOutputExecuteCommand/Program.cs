using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppVeyorOutputExecuteCommand {

	class Program {
		static void Main( string[] args ) {
			//ExecuteCommand( "node app.js", @"C:\Users\bergw\_Projekte\MapBox\AppVeyorOutput" );
			//ExecuteCommand( "npm test 1>logger.txt 2>&1", @"C:\dev2\node-srs" );
			//ExecuteCommand( @"node ""C:\Program Files (x86)\nodejs\node_modules\npm\bin\npm-cli.js"" test", @"C:\dev2\node-srs" );
			ExecuteCommand( "npm test", @"C:\dev2\node-srs" );
			Console.WriteLine( "FINISHED" );
			Console.ReadKey();
		}

		private static int ExecuteCommand( string command, string workingDirectory ) {

			using (Process p = new Process()) {
				p.StartInfo = new ProcessStartInfo( "cmd", "/c " + command ) {
					UseShellExecute = false,
					CreateNoWindow=true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = workingDirectory,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8
				};

				p.OutputDataReceived += ( sender, e ) => {
					if (e.Data != null) {
						Console.WriteLine( e.Data );
					} else {
						Console.WriteLine( "DATA: NULL" );
					}
				};
				p.ErrorDataReceived += ( sender, e ) => {
					if (e.Data != null) {
						Console.WriteLine( e.Data );
					} else {
						Console.WriteLine( "ERROR: NULL" );
					}
				};

				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.WaitForExit();

				return p.ExitCode;
			}
		}


		private static CountdownEvent countdownEvent;
		private static StringBuilder sbOut;
		private static bool exited = false;
		private static int ExecuteCommand_2( string command, string workingDirectory ) {

			sbOut = new StringBuilder();
			countdownEvent = new CountdownEvent( 2 );

			using (Process p = new Process()) {
				p.StartInfo = new ProcessStartInfo( "cmd", "/c " + command ) {
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = workingDirectory,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8,
					CreateNoWindow = true
				};

				p.EnableRaisingEvents = true;
				p.OutputDataReceived += new DataReceivedEventHandler( DataReceived );
				p.ErrorDataReceived += new DataReceivedEventHandler( DataReceived );
				p.Exited += new EventHandler( pexited );

				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();

				//using (StreamReader sr = new StreamReader(p.StandardOutput.BaseStream, Encoding.UTF8)) {
				//	while (sr.BaseStream.CanRead) {
				//		Console.WriteLine( sr.ReadLine() );
				//	}
				//}
				//while (p.StandardOutput.BaseStream.CanRead) {
				//	string l = p.StandardOutput.ReadLine();
				//	Trace.WriteLine( l );
				//	Console.WriteLine( l);
				//}
				p.WaitForExit( -1 );
				while (!p.HasExited) { Thread.Sleep( 1000 ); }
				//Console.WriteLine( p.StandardOutput.ReadToEnd() );
				//Console.WriteLine( p.StandardError.ReadToEnd() );
				//countdownEvent.Wait();

				while (!exited) { }

				//Console.Write( sbOut.ToString() );
				return p.ExitCode;
			}
		}

		static void pexited( object sender, EventArgs e ) {
			exited = true;
		}


		private static void DataReceived( object sender, DataReceivedEventArgs e ) {
			if (string.IsNullOrWhiteSpace( e.Data )) {
				Trace.WriteLine( "NULL" );
			}
			Trace.WriteLine( e.Data );
			if (e.Data != null) {
				//lock (sbOut) { sbOut.AppendLine( e.Data ); }
				Console.WriteLine( e.Data );
			} else {
				// end of stream
				//countdownEvent.AddCount();
				countdownEvent.Signal();
			}
		}











	}
}
