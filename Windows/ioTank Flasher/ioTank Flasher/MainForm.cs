/*
 * Created by John Spounias (d3cline) @@ OBJECT SYNDICATE LLC
 */
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ioTank_Flasher
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
            this.Load += MainForm_Load;
		}

    void MainForm_Load(object sender, EventArgs e)
    {
    	string executableLocation = Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location);
        var ports = SerialPort.GetPortNames();
        serial.DataSource = ports;
	    serial.SelectedIndex = ports.Count() -1;
        var settings_f = File.ReadAllLines(executableLocation +"\\data\\f");
        int count = 0;
		foreach (var element in settings_f)
        {	
			count += 1;
			if(count == 1){ssid.Text = element;}
			if(count == 2){password.Text = element;}
			if(count == 3){Cloud.Checked = Boolean.Parse(element);}
			if(count == 4){token.Text = element;}
			if(count == 5){host.Text = element;}
			if(count == 6){endpoint.Text = element;}

		}
	
   }

		void FlashClick(object sender, EventArgs e)
		{
			string executableLocation = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);
			using (var objWriter = new StreamWriter(executableLocation +"\\data\\f"))
			{
		    objWriter.Write(ssid.Text+Environment.NewLine);
		    objWriter.Write(password.Text+Environment.NewLine);
		    objWriter.Write(Cloud.Checked+Environment.NewLine);
		    objWriter.Write(token.Text+Environment.NewLine);
		    objWriter.Write(host.Text+Environment.NewLine);
			objWriter.Write(endpoint.Text+Environment.NewLine);
			}
					

			string strMkspiffsCmd = executableLocation +"\\mkspiffs.exe";
			string strEsptoolCmd = executableLocation + "\\esptool.exe";

			var mkspiffs = new Process();
			mkspiffs.StartInfo.FileName = strMkspiffsCmd;
			mkspiffs.StartInfo.Arguments = " -c data -p 256 -b 8192 out.bin";
			mkspiffs.Start();
			System.Threading.Thread.Sleep(500);


			var esptool = new Process();
			esptool.StartInfo.FileName = strEsptoolCmd;
			esptool.StartInfo.Arguments = "  -cd nodemcu -cb 115200 -cp "+serial.Text+" -ca 0x300000 -cf out.bin";
			esptool.Start();
		}

	
		
		static Regex isGuid = new Regex(@"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", RegexOptions.Compiled);
		static Regex isIP = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

		bool IsGuid(string candidate)
		{
		   if (candidate != null)
		   {
		   	//Debug.Write(candidate);
		      if (isGuid.IsMatch(candidate))
		      {
		         return true;
		      }
		   }
		
		   return false;
		}
		
		bool IsIP(string candidate)
		{
		   if (candidate != null)
		   {
		   	//Debug.Write(candidate);
		      if (isIP.IsMatch(candidate))
		      {
		         return true;
		      }
		   }
		
		   return false;
		}	

		
	
		void MainFormLoad(object sender, EventArgs e)
		{
	
		}
		void Button1Click(object sender, EventArgs e)
		{
			Process.Start("https://www.objectsyndicate.com/docs/");

		}
		void Button3Click(object sender, EventArgs e)
		{
		var port = new SerialPort(serial.Text, 115200, Parity.None, 8, StopBits.One);
		port.ReadTimeout = 2000;
  		try 
        {	
		port.Open();
		string data = port.ReadLine();
		port.Close();


		if (IsIP(data))
		{
			status.Text = data;
		}
		else{
			port.Open();
			data = port.ReadLine();
			port.Close();

			if (IsIP(data))
			{
				status.Text = data;
			}
			else{
				port.Open();
				data = port.ReadLine();
				port.Close();

				if (IsIP(data))
				{
					status.Text = data;
				}
				else{

		status.Text = data;
				}
				
			}
		
		}

		
		
		port.Close();
		}

  		catch (TimeoutException ex)
        {
        status.Text = "Communication error";
        }
        catch (UnauthorizedAccessException ex)
        {
        status.Text = "Communication error";
        }
    
  }
		void FactoryClick(object sender, EventArgs e)
		{
			string executableLocation = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);
			
			string strEsptoolCmd = executableLocation + "\\esptool.exe";
			var port = new SerialPort(serial.Text, 115200, Parity.None, 8, StopBits.One);
			port.Close();
			var firmware = new Process();
			firmware.StartInfo.FileName = strEsptoolCmd;
			firmware.StartInfo.Arguments = " -cd nodemcu -cb 115200 -cp "+serial.Text+" -ca 0x000000 -cf ioTank.bin";
			firmware.Start();
			firmware.WaitForExit();
			System.Threading.Thread.Sleep(5000);
			

			port.Close();
			
			var spiff = new Process();
			spiff.StartInfo.FileName = strEsptoolCmd;
			spiff.StartInfo.Arguments = " -cd nodemcu -cb 115200 -cp "+serial.Text+" -ca 0x300000 -cf ioTank.spiffs.bin";
			spiff.Start();
			

			port.Close();
		}
	}
}
