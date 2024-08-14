using ClinicDent2.ViewModel;
using ClinicDentClientCommon.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TL;
using WTelegram;

namespace ClinicDent2
{
    public static class TelegramMessageSender
    {
        public static Client client;
        private static string enteredVerificationCode;
        public static async Task InitAsync()
        {
            enteredVerificationCode = "";
            client = new Client(Config);
            await client.LoginUserIfNeeded().ConfigureAwait(false);
        }
        public async static Task<InputPeer> SendTextMessage(string name, string receiverPhoneNumber, string message)
        {
            Contacts_ResolvedPeer peer = null;
            try
            {
                peer = await client.Contacts_ResolvePhone(receiverPhoneNumber).ConfigureAwait(false);
            }
            catch(RpcException)
            {
                throw new Exception($"Не вдалось знайти контакт з номером \'{receiverPhoneNumber}\'");
            }
            await client.SendMessageAsync(peer, message).ConfigureAwait(false);
            //int[] contactIds = await client.Contacts_GetContactIDs().ConfigureAwait(false);
            //if (contactIds.Any(n => n == peer.peer.ID) == false)
            //{
            //    await client.Contacts_AddContact(peer.User, name, "", $"+{receiverPhoneNumber}").ConfigureAwait(false);
            //}
            return peer;
        }
        public static async Task SendStageAsync(string name, string phoneNumber, StageViewModel stage)
        {
            if (client == null)
            {
                await TelegramMessageSender.InitAsync();
            }
            string message = $"{stage.StageDatetime}{Environment.NewLine}{stage.Title}{Environment.NewLine}Оплачено {stage.Payed}/{stage.Price} грн.";
            InputPeer user = await SendTextMessage(name, phoneNumber, message).ConfigureAwait(false);

            for (int i = 0; i < stage.Images.Count; i++)
            {
                if (stage.Images[i].image.OriginalBytes == null)
                {
                    stage.Images[i].image.OriginalBytes = await HttpService.GetImageOriginalBytes(stage.Images[i].Id);
                }
                string extension = GetImageExtension(stage.Images[i].image.OriginalBytes);
                string nameWithExtension = $"{stage.Title} ({i+1}){extension}";
                string mimeType = GetImageMimeType(stage.Images[i].image.OriginalBytes);

                InputFileBase fileResult = await client.UploadFileAsync(new MemoryStream(stage.Images[i].image.OriginalBytes), nameWithExtension).ConfigureAwait(false);
                await client.SendMediaAsync(user, "", fileResult, mimeType).ConfigureAwait(false);
            }
        }
        static string Config(string what)
        {
            switch (what)
            {
                case "api_id": return IniService.GetPrivateString("Telegram", "apiId");
                case "api_hash": return IniService.GetPrivateString("Telegram", "apiHash");
                case "phone_number": return IniService.GetPrivateString("Telegram", "phoneNumber");
                case "verification_code":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WindowEnterMessage windowEnterMessage = new WindowEnterMessage();
                        windowEnterMessage.ConfirmPressed += WindowEnterMessage_ConfirmPressed;
                        windowEnterMessage.CancelPressed += WindowEnterMessage_CancelPressed;
                        windowEnterMessage.ShowDialog();
                    });
                    return enteredVerificationCode;
                case "password": return IniService.GetPrivateString("Telegram", "optionalPassword");
                default: return null;                  // let WTelegramClient decide the default config
            }
        }
        private static void WindowEnterMessage_CancelPressed(WindowEnterMessage sender)
        {
            Application.Current.Dispatcher.Invoke(() => sender.Close());
            enteredVerificationCode = "";

        }

        private static void WindowEnterMessage_ConfirmPressed(WindowEnterMessage sender, string text)
        {
            Application.Current.Dispatcher.Invoke(() => sender.Close());
            enteredVerificationCode = text;
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
