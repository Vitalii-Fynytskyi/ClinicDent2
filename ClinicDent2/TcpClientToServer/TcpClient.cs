using ClinicDent2.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace ClinicDent2.TcpClientToServer
{
    public delegate void ScheduleRecordStateUpdatedDelegate(int recordId, SchedulePatientState newState, string dateTime, int cabinetId);
    public delegate void ScheduleRecordDeletedDelegate(int recordId, string dateTime, int cabinetId);
    public delegate void ScheduleCabinetCommentUpdatedDelegate(string datetime, int cabinetId, string newComment);

    public delegate void ScheduleRecordCommentUpdatedDelegate(int recordId, string newComment, string dateTime, int cabinetId);
    public class TcpClient
    {
        public event EventHandler<Schedule> ScheduleRecordAdded;
        public event EventHandler<Schedule> ScheduleRecordUpdated;
        public event ScheduleRecordDeletedDelegate ScheduleRecordDeleted;
        public event ScheduleRecordStateUpdatedDelegate ScheduleRecordStateUpdated;
        public event ScheduleRecordCommentUpdatedDelegate ScheduleRecordCommentUpdated;
        public event ScheduleCabinetCommentUpdatedDelegate SchedlueCabinetCommentUpdated;


        bool isClientOutOfDate = false;
        string commandDelimeter = new string((char)1, 1);
        string packetDelimeter = new string((char)2, 1);
        string serverIp;
        int serverPort;
        string stringIn = "";
        byte[] buffer = new byte[1024];
        string[] preSplitArray;
        string[] splitArray;
        public bool Connected = false;
        public bool Authhorized = false;
        Socket socket;
        public object socketLocker = new object();
        Thread socketDataListenerThread = null;
        public string clientVersion = "0.01";
        string messageToServer;
        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        System.Timers.Timer timerPingServer;
        public TcpClient()
        {
            //read server address from ini file
            string serverAddress = IniService.GetPrivateString("Settings", "ServerAddress");
            Uri uri= new Uri(serverAddress);
            serverIp = uri.Host;
            serverPort = Convert.ToInt32(IniService.GetPrivateString("Settings", "TcpPort"));
            timerPingServer=new System.Timers.Timer();
            timerPingServer.Interval = 100 * 1000; //100 sec
            timerPingServer.AutoReset = true;
            timerPingServer.Elapsed += TimerPingServer_Elapsed;
        }

        private void TimerPingServer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SetMessageToServerAndSend("p");
        }

        void SetMessageToServer(params string[] commands)
        {
            messageToServer = String.Join(commandDelimeter, commands) + commandDelimeter + clientVersion + packetDelimeter;
        }
        void SendToServer()
        {
            if (Connected == true)
                socket.Send(Encoding.UTF8.GetBytes(messageToServer));
            else
                MessageBox.Show("З'єднання закрито. Дія неможлива");
        }
        void SetMessageToServerAndSend(params string[] commands)
        {
            lock (socketLocker)
            {
                SetMessageToServer(commands);
                SendToServer();
            }
        }
        public void ConnectToServer()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(serverIp);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, serverPort);
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.Connect(ipEndPoint);
                Connected = true;
                socketDataListenerThread = new Thread(socketDataListener);
                socketDataListenerThread.Start();
                SetMessageToServerAndSend("login", Options.CurrentDoctor.EncodedJwt);
            }
            catch(Exception e)
            {
                socket?.Close();
                socketDataListenerThread?.Abort();
                throw e;
            }
        }
        public void socketDataListener()
        {
            try
            {
                while (true)
                {
                    int receivedBytes = socket.Receive(buffer);

                    // Decode the received bytes into characters
                    int charCount = utf8Decoder.GetCharCount(buffer, 0, receivedBytes);
                    char[] chars = new char[charCount];
                    utf8Decoder.GetChars(buffer, 0, receivedBytes, chars, 0);

                    stringIn += new string(chars);
                    if (stringIn.EndsWith(packetDelimeter))
                    {
                        preSplitArray = stringIn.Split(new char[] { packetDelimeter[0] }, StringSplitOptions.None);
                        stringIn = "";
                        foreach (string packet in preSplitArray)
                        {
                            recodeString(packet);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Write(ex.Message + ex.ToString()); ;
                if(Connected == true)
                {
                    DisconnectFromServer(false);
                    return;
                }
            }
        }
        public void DisconnectFromServer(bool isPlanned = true)
        {
            timerPingServer.Stop();
            Authhorized = false;
            Connected = false;
            socket?.Close();
            if (isPlanned == false)
            {
                //try to reconnect
                ConnectToServer();
            }
        }

        private void recodeString(string packet)
        {
            splitArray = packet.Split(new char[] { commandDelimeter[0] }, StringSplitOptions.None);
            switch (splitArray[0])
            {
                case "successLogin":
                    answerSuccessLogin();
                    break;
                case "scheduleRecordAdded":
                    answerScheduleRecordAdded(splitArray[1], splitArray[2], splitArray[3], splitArray[4], splitArray[5], splitArray[6], splitArray[7], splitArray[8], splitArray[9], splitArray[10], splitArray[11], splitArray[12], splitArray[13], splitArray[14], splitArray[15]);
                    break;
                case "scheduleRecordDeleted":
                    answerScheduleRecordDeleted(splitArray[1], splitArray[2], splitArray[3]);
                    break;
                case "scheduleRecordUpdated":
                    answerScheduleRecordUpdated(splitArray[1], splitArray[2], splitArray[3], splitArray[4], splitArray[5], splitArray[6], splitArray[7], splitArray[8], splitArray[9], splitArray[10]);
                    break;
                case "scheduleRecordStateUpdated":
                    answerScheduleRecordStateUpdated(splitArray[1], splitArray[2], splitArray[3], splitArray[4]);
                    break;
                case "scheduleRecordCommentUpdated":
                    answerScheduleRecordCommentUpdated(splitArray[1], splitArray[2], splitArray[3], splitArray[4]);
                    break;
                case "scheduleCabinetCommentUpdated":
                    answerScheduleCabinetCommentUpdated(splitArray[1], splitArray[2], splitArray[3]);
                    break;
                case "clientOutOfDate":
                    answerClientOutOfDate();
                    break;
                case "wrongLogin":
                    answerWrongLogin();
                    break;
                case "stagePayInfoUpdated":
                    answerStagePayInfoUpdated(splitArray[1]);
                    break;
            }
        }
        #region Answers
        private void answerStagePayInfoUpdated(string updatedStagesContent)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                Options.MainWindow.mainMenu.browserControl.NotifyAllTabs(NotificationCodes.ScheduleStagesPaymentUpdated, updatedStagesContent);
            }));
            
        }
        private void answerScheduleCabinetCommentUpdated(string datetime, string cabinetId, string comment)
        {
            SchedlueCabinetCommentUpdated?.Invoke(datetime, Int32.Parse(cabinetId), comment);
        }
        private void answerScheduleRecordCommentUpdated(string updatedRecordId, string newComment, string dateTime, string cabinetId)
        {
            ScheduleRecordCommentUpdated?.Invoke(Int32.Parse(updatedRecordId), newComment, dateTime, Int32.Parse(cabinetId));

        }

        private void answerScheduleRecordStateUpdated(string updatedRecordId, string newState, string dateTime, string cabinetId)
        {
            ScheduleRecordStateUpdated?.Invoke(Int32.Parse(updatedRecordId), (SchedulePatientState)Int32.Parse(newState), dateTime, Int32.Parse(cabinetId));
        }

        private void answerScheduleRecordDeleted(string deletedRecordId, string dateTime, string cabinetId)
        {
            ScheduleRecordDeleted?.Invoke(Int32.Parse(deletedRecordId), dateTime, Int32.Parse(cabinetId));
        }

        private void answerScheduleRecordUpdated(string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9, string v10)
        {
            Schedule s = new Schedule(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10);
            ScheduleRecordUpdated?.Invoke(this, s);
        }

        private void answerScheduleRecordAdded(string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9, string v10,string priceSum, string payedSum, string messagerSentState, string doctorsId, string expensesSum)
        {
            Schedule s = new Schedule(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, priceSum, payedSum, messagerSentState, doctorsId, expensesSum);
            ScheduleRecordAdded?.Invoke(this, s);

        }
        private void answerSuccessLogin()
        {
            Authhorized= true;
            timerPingServer.Start();
        }
        private void answerWrongLogin()
        {
            DisconnectFromServer();
            MessageBox.Show("Не вдалось підключитись до сервера. Запустіть заново вікно");
        }

        private void answerClientOutOfDate()
        {
            DisconnectFromServer();
            isClientOutOfDate= true;
            MessageBox.Show("Ваша версія програми застаріла та не сумісна з сервером");

        }
        #endregion
        #region Commands
        public void AddRecord(Schedule s)
        {
            string patientIdToSend = "<null>";
            if (s.PatientId != null)
            {
                patientIdToSend = s.PatientId.ToString();
            }
            SetMessageToServerAndSend("scheduleAddRecord", s.Id.ToString(), s.StartDatetime, s.EndDatetime, s.Comment, patientIdToSend, s.DoctorId.ToString(), s.PatientName, s.CabinetId.ToString(), s.CabinetName,((int)s.State).ToString());
        }
        public void DeleteRecord(int recordId)
        {
            SetMessageToServerAndSend("scheduleDeleteRecord", recordId.ToString());
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordDeleted, recordId);
        }
        public void UpdateRecord(Schedule s)
        {
            string patientIdToSend = "<null>";
            if (s.PatientId != null)
            {
                patientIdToSend = s.PatientId.ToString();
            }
            SetMessageToServerAndSend("scheduleUpdateRecord", s.Id.ToString(), s.StartDatetime, s.EndDatetime, s.Comment, patientIdToSend, s.DoctorId.ToString(), s.PatientName, s.CabinetId.ToString(), s.CabinetName, ((int)s.State).ToString());
            Options.MainWindow.mainMenu.browserControl.NotifyOtherTabs(NotificationCodes.ScheduleRecordUpdated, s);

        }
        public void UpdateRecordState(int recordId, SchedulePatientState newState)
        {
            SetMessageToServerAndSend("scheduleUpdateRecordState",recordId.ToString(),Convert.ToString((int)newState));

        }
        public void UpdateRecordComment(int recordId, string newComment)
        {
            SetMessageToServerAndSend("scheduleUpdateRecordComment", recordId.ToString(), newComment);

        }

        internal void UpdateCabinetComment(string dateTime, int cabinetId, string cabinetComment)
        {
            SetMessageToServerAndSend("scheduleUpdateCabinetComment", dateTime, cabinetId.ToString(), cabinetComment);

        }
        #endregion
    }
}
