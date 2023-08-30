using ClinicDent2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Exceptions;
using TLSharp.Core.Utils;

namespace ClinicDent2
{
    public static class TelegramMessageSender
    {
        public static int apiId;
        public static string apiHash;
        public static string phoneNumber;
        public static string hash;
        public static TelegramClient client;
        /// <summary>
        /// This function initialize and authorize telegram client if required. Always begin TelegramMessageSender usage after calling Init 
        /// </summary>
        public static async Task InitAsync()
        {
            apiId = Int32.Parse(IniService.GetPrivateString("Telegram", "apiId"));
            apiHash = IniService.GetPrivateString("Telegram", "apiHash");
            phoneNumber = IniService.GetPrivateString("Telegram", "phoneNumber");
            client = new TelegramClient(apiId, apiHash);
            await client.ConnectAsync().ConfigureAwait(false);

            if (!client.IsUserAuthorized())
            {
                hash = await client.SendCodeRequestAsync(phoneNumber).ConfigureAwait(false);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WindowEnterMessage windowEnterMessage = new WindowEnterMessage();
                    windowEnterMessage.ConfirmPressed += WindowEnterMessage_ConfirmPressed;
                    windowEnterMessage.CancelPressed += WindowEnterMessage_CancelPressed;
                    windowEnterMessage.ShowDialog();
                });
            }
        }
        private static void WindowEnterMessage_CancelPressed(WindowEnterMessage sender)
        {
            Application.Current.Dispatcher.Invoke(() => sender.Close());
            
        }

        private static async void WindowEnterMessage_ConfirmPressed(WindowEnterMessage sender, string text)
        {
            await TelegramMessageSender.SendCodeRequest(text).ConfigureAwait(false);
            Application.Current.Dispatcher.Invoke(() => sender.Close());
        }
        public async static Task SendCodeRequest(string code)
        {
            try
            {
                await client.MakeAuthAsync(phoneNumber, hash, code).ConfigureAwait(false); ;
            }
            catch (CloudPasswordNeededException)
            {
                var password = IniService.GetPrivateString("Telegram", "optionalPassword");
                var passwordSetting = await client.GetPasswordSetting().ConfigureAwait(false); ;
                await client.MakeAuthWithPasswordAsync(passwordSetting, password).ConfigureAwait(false);
            }
        }
        public async static Task<TLUser> SendTextMessage(string name, string receiverPhoneNumber, string message)
        {
            // this is because the contacts in the address come without the "+" prefix
            var normalizedNumber = receiverPhoneNumber.StartsWith("+") ?
                receiverPhoneNumber.Substring(1, receiverPhoneNumber.Length - 1) :
                receiverPhoneNumber;

            var result = await client.GetContactsAsync().ConfigureAwait(false); ;

            TLUser user = result.Users
                .OfType<TLUser>()
                .FirstOrDefault(x => x.Phone == normalizedNumber);

            if(user != null)
            {
                await client.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, message).ConfigureAwait(false); ;
            }
            else
            {
                user = await ImportContact(name, "", receiverPhoneNumber).ConfigureAwait(false); ;
                if(user == null)
                {
                    throw new Exception("Не вдалось відправити повідомлення. Не зареєстровано Telegram");
                }
                await client.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, message).ConfigureAwait(false); ;
            }
            return user;
        }
        public async static Task<TLUser> ImportContact(string firstName, string lastName, string phoneNumber)
        {
            var imported = await client.ImportContactsAsync(new List<TLInputPhoneContact> {
            new TLInputPhoneContact {
                ClientId = 1,
                Phone = phoneNumber,
                FirstName = firstName,
                LastName = lastName
            }}).ConfigureAwait(false); ;
            if (imported.Users.Count > 0)
            {
                var user = (TLUser)imported.Users[0];
                return user;
            }
            return null;
        }
        public static async Task SendStageAsync(string name, string phoneNumber, StageViewModel stage)
        {
            if (client == null)
            {
                await TelegramMessageSender.InitAsync();
            }
            if (client.IsConnected == false)
            {
                await client.ConnectAsync();
            }
            string message = $"{stage.StageDatetime}{Environment.NewLine}{stage.Title}{Environment.NewLine}Оплачено {stage.Payed}/{stage.Price} грн.";
            TLUser user = await SendTextMessage(name, phoneNumber, message).ConfigureAwait(false);

            for(int i=0;i< stage.Images.Count; i++)
            {
                if (stage.Images[i].image.OriginalBytes == null)
                {
                    stage.Images[i].image.OriginalBytes = HttpService.GetImageOriginalBytes(stage.Images[i].Id);
                }
                string extension = GetImageExtension(stage.Images[i].image.OriginalBytes);
                string nameWithExtension = stage.Images[i].FileName + extension;
                string mimeType = GetImageMimeType(stage.Images[i].image.OriginalBytes);

                var fileResult = await client.UploadFile(nameWithExtension, new StreamReader(new MemoryStream(stage.Images[i].image.OriginalBytes))).ConfigureAwait(false);

                var attributes = new TLVector<TLAbsDocumentAttribute>
                {
                    new TLDocumentAttributeFilename { FileName = $"{stage.Title} ({i+1}){extension}" }
                };

                await client.SendUploadedDocument(new TLInputPeerUser() { UserId = user.Id }, fileResult, "", mimeType, attributes).ConfigureAwait(false);
            }
        }
        public static string GetImageExtension(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 4) // We'll need at least 4 bytes to determine type
                throw new ArgumentException("Invalid image byte array.");
            // JPEG
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                return ".jpg";
            // PNG
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return ".png";
            // BMP
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
                return ".bmp";
            // TIFF (little endian)
            if (imageBytes[0] == 0x49 && imageBytes[1] == 0x49 && imageBytes[2] == 0x2A && imageBytes[3] == 0x00)
                return ".tif";
            // TIFF (big endian)
            if (imageBytes[0] == 0x4D && imageBytes[1] == 0x4D && imageBytes[2] == 0x00 && imageBytes[3] == 0x2A)
                return ".tif";
            throw new ArgumentException("Unknown image type or unsupported format.");
        }
        public static string GetImageMimeType(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 4) // We'll need at least 4 bytes to determine type
                throw new ArgumentException("Invalid image byte array.");
            // JPEG
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                return "image/jpeg";
            // PNG
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return "image/png";
            // BMP
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
                return "image/bmp";
            // TIFF (little endian)
            if (imageBytes[0] == 0x49 && imageBytes[1] == 0x49 && imageBytes[2] == 0x2A && imageBytes[3] == 0x00)
                return "image/tiff";
            // TIFF (big endian)
            if (imageBytes[0] == 0x4D && imageBytes[1] == 0x4D && imageBytes[2] == 0x00 && imageBytes[3] == 0x2A)
                return "image/tiff";
            throw new ArgumentException("Unknown image type or unsupported format.");
        }
    }
}