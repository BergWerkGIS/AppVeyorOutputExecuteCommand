using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppVeyorOutputExecuteCommand {

	class Program {
		static void Main( string[] args ) {
			ExecuteCommand( "node app.js", @"C:\Users\bergw\_Projekte\MapBox\AppVeyorOutput" );
			Console.ReadKey();
		}

		private static int ExecuteCommand_ORGINAL( string command, string workingDirectory ) {

			using (Process p = new Process()) {
				p.StartInfo = new ProcessStartInfo( "cmd", "/c " + command ) {
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = workingDirectory,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8
				};

				p.OutputDataReceived += ( sender, e ) => {
					if (e.Data != null) {
						Console.WriteLine( e.Data );
						Console.Out.Flush();
					}
				};
				p.ErrorDataReceived += ( sender, e ) => {
					if (e.Data != null) {
						Console.WriteLine( e.Data );
						Console.Out.Flush();
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
		private static int ExecuteCommand( string command, string workingDirectory ) {

			sbOut = new StringBuilder();
			countdownEvent = new CountdownEvent( 2 );

			using (Process p = new Process()) {
				p.StartInfo = new ProcessStartInfo( "cmd", "/c " + command ) {
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = workingDirectory,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8
				};

				p.OutputDataReceived += new DataReceivedEventHandler( DataReceived );
				p.ErrorDataReceived += new DataReceivedEventHandler( DataReceived );

				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.WaitForExit();

				Console.Write( sbOut.ToString() );
				return p.ExitCode;
			}
		}


		private static void DataReceived( object sender, DataReceivedEventArgs e ) {
			if (e.Data != null) {
				lock (sbOut) {
					sbOut.AppendLine( e.Data );
				}
			} else {
				// end of stream
				countdownEvent.AddCount();
			}
		}











	}
}
