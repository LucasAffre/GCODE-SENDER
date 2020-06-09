using System;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gcode_Sender
{
    public partial class Form1 : Form
    {
        static bool lectura;
        static bool _continue;
        static SerialPort _serialPort;
        string message;
        int countlines;
        Thread readThread = new Thread(Read);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                cmbPort.Items.Add(s);
            }
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                cmbPortParity.Items.Add(s);
            }
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                cmbHandshake.Items.Add(s);
            }
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            btnConectar.Enabled = false;
            btnCerrar.Enabled = false;

            _serialPort = new SerialPort();
            _serialPort.PortName = cmbPort.SelectedItem.ToString();
            _serialPort.BaudRate = int.Parse(cmbBaudRate.SelectedItem.ToString());
            _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), cmbPortParity.SelectedItem.ToString(), true);
            _serialPort.DataBits = int.Parse(cmbDataBits.SelectedItem.ToString());
            _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.SelectedItem.ToString(), true);
            _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), cmbHandshake.SelectedItem.ToString(), true);

            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();

            _continue = true;
            readThread.Start();

            string fileName = nombreArchivo.Text;

            string text = File.ReadAllText(fileName);
            string[] lines = text.Split('\n');
            lines = lines.Skip(6).ToArray();
            countlines = lines.Count();

            GcodeProgress.Maximum = countlines;
            GcodeProgress.Step = 1;

            foreach (string line in lines)
            {
                GcodeProgress.PerformStep();
                Sender.Text = line;
                message = line;
                _serialPort.WriteLine(String.Format("{0}", message));
                do
                {
                    if (lectura == true)
                    {
                        break;
                    }

                } while (true);
                lectura = false;

            }
            readThread.Join();
            _serialPort.Close();
        }
        public static void Read()
        {

            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    //Console.WriteLine(message);
                    lectura = true;

                }
                catch (TimeoutException) { }
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            _continue = false;
            btnCerrar.Enabled = false;
            btnConectar.Enabled = true;
        }

        private void BtnArchivo_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nombreArchivo.Text = openFileDialog1.FileName;
            }
        }

        
    }
}
