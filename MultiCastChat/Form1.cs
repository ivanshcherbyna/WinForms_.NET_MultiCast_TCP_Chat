using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiCastChat
{
    public partial class Form1 : Form
    {
        bool alive = false; //означатет будет ли работать поток для приема
        UdpClient client;
        //для реально разных компьютеров необх. вынести порты в форму, они должны оличаться (отправка и получение)
        const int LOCALPORT = 8001;// порт для приема сообщений
        const int REMOTEPORT = 8001;// порт для отправки сообщений
        const int TTL = 20; // time to leave
        const string HOST = "235.5.5.1";//хост для групповой рассылки
        string userName;//имя пользователя
        IPAddress groupAddress;//адресс для групповой рассылки
        
       // List<String> peolpe = null;
        public Form1()
        {
           
            InitializeComponent();
           
            btnExit.Enabled = false;
            btnSend.Enabled = false;
            tbChat.ReadOnly = true;
          //  peolpe = new List<string>();
            groupAddress = IPAddress.Parse(HOST);
            
        }
        
        private void btnEnter_Click(object sender, EventArgs e)
        {
            userName = tbName.Text;
            tbName.ReadOnly = true;
            try
            {
               
                client = new UdpClient(LOCALPORT); // создаем клиента
                //присоединяемся к групповой рассылке
                client.JoinMulticastGroup(groupAddress, TTL);
                Task.Factory.StartNew(RecieveMessages);

                string message = userName + " вошел в чат";
                lbPeople.Items.Add(userName);
                lbPeople.Update();
                byte[] data = Encoding.Default.GetBytes(message); //передаем всегда желательно байты, переводи их в байты чтобы знать их размер
                client.Send(data, data.Length, HOST, REMOTEPORT);

                btnEnter.Enabled = false;
                btnExit.Enabled = true;
                btnSend.Enabled = true;
               
            }
            catch (Exception ex) { MessageBox.Show(ex.StackTrace.ToString());}
        }

        private void RecieveMessages() //метод приема сообщений
        {
            alive = true;
            try
            {
                IPEndPoint remoteIp = null;
                byte[] data = client.Receive(ref remoteIp);
                string message = Encoding.Default.GetString(data);

                //добавление сообщения в текстовое поле
                Invoke(new Action(() =>
                {
                    string time = DateTime.Now.ToShortTimeString();
                    tbChat.Text = time + " " + message + "\r\n" + tbChat.Text;
                }));
            }
            catch(ObjectDisposedException ex)
            {
                if (!alive) { return; throw; }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message);}
        }

        private void exit()
        {
            try
            {
                string message = userName + " покидает чат ";
                byte[] data = Encoding.Default.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                RecieveMessages();
                client.DropMulticastGroup(groupAddress);//отсоединяемся от групповой рассылки
                alive = false;
                tbName.ReadOnly = false;
                client.Close();

                btnEnter.Enabled = true;
                btnExit.Enabled = false;
                btnSend.Enabled = false;
                tbName.Enabled = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.StackTrace.ToString());}
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            exit();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(RecieveMessages);

                string message;
                message=userName+": "+ tbMessage.Text;
                //string message = String;
                byte[] data = Encoding.Default.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                tbMessage.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if(btnExit.Enabled==false) this.Close();
            exit();
            
        }

        private void lbPeople_MouseClick(object sender, MouseEventArgs e)
        {
            if (lbPeople.Items != null)
            {
                tbMessage.Text += lbPeople.SelectedItem.ToString() + ", ";
            }
            else MessageBox.Show("Выбирите пожалуйта кому отправлять соощение хотите");


        }
    }
}
