using ClinicDent2.Model;
using ClinicDent2.RequestAnswers;
using ClinicDent2.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

namespace ClinicDent2
{
    public static class HttpService
    {
        static HttpClient httpClient;
        static MediaTypeFormatter[] bsonFormatting;
        static MediaTypeFormatter BsonFormatter;
        static MediaTypeWithQualityHeaderValue bsonHeaderValue;
        static MediaTypeWithQualityHeaderValue octetStreamHeaderValue;
        static HttpService()
        {
            httpClient = new HttpClient();
            BsonFormatter = new BsonMediaTypeFormatter();
            bsonFormatting = new MediaTypeFormatter[] { BsonFormatter };
            httpClient.BaseAddress = new Uri(IniService.GetPrivateString("Settings", "ServerAddress"));
            bsonHeaderValue = new MediaTypeWithQualityHeaderValue("application/bson");
            octetStreamHeaderValue = new MediaTypeWithQualityHeaderValue("application/octet-stream");

        }
        public static ScheduleRecordsForDayInCabinet GetSchedule(DateTime date, string cabinetId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            string dateStr = date.ToString(Options.DateTimePattern);



            HttpResponseMessage responseMessage = httpClient.GetAsync($"Schedule/getRecordsForDay/{dateStr}/{cabinetId}").Result;
            if (responseMessage.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<Schedule> GetSchedule(date={date.ToString()}; cabinetId={cabinetId}). Status code: {responseMessage.StatusCode}");
            }
            ScheduleRecordsForDayInCabinet receivedRecords = responseMessage.Content.ReadAsAsync<ScheduleRecordsForDayInCabinet>(bsonFormatting).Result;
            return receivedRecords;
        }
        internal static void PutPatientCurePlan(int patientIdToSet, string curePlanToSet)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            ChangeCurePlanRequest r = new ChangeCurePlanRequest()
            {
                CurePlan = curePlanToSet,
                PatientId = patientIdToSet
            };
            HttpResponseMessage result = httpClient.PutAsync($"Patients/changeCurePlan", r, BsonFormatter).Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void PutPatientCurePlan(patientIdToSet={patientIdToSet},curePlanToSet={curePlanToSet}). Status code: {result.StatusCode}");
            }
        }
        internal static List<Stage> GetPatientStages(int patientId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Stages/{patientId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<Stage> GetPatientStages(patientId = {patientId}). Status code: {result.StatusCode}");
            }
            List<Stage> stages = result.Content.ReadAsAsync<List<StageDTO>>(bsonFormatting).Result.Select(d => new Stage(d)).ToList();
            return stages;
        }
        public static Patient GetPatient(int patientId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Patients/{patientId}").Result;
            if (result.IsSuccessStatusCode)
            {
                Patient receivedPatient = result.Content.ReadAsAsync<Patient>(bsonFormatting).Result;
                return receivedPatient;
            }
            else
            {
                throw new Exception($"Patient GetPatient(patientId = {patientId}). Status code: {result.StatusCode}");
            }
        }
        public static PatientsToClient GetPatients(string selectedStatus, string selectedSortDescription, int selectedPage, int patientsPerPage, string searchText)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Patients/{selectedStatus}/{selectedSortDescription}/{selectedPage}/{patientsPerPage}/{searchText}").Result;
            if (result.IsSuccessStatusCode)
            {
                return result.Content.ReadAsAsync<PatientsToClient>(bsonFormatting).Result;
            }
            else
            {
                throw new Exception($"PatientsToClient GetPatients( ... ). Status code: {result.StatusCode}");
            }
        }
        public static PatientsToClient GetPatientsByImage(int imageId,string selectedStatus, string selectedSortDescription, int selectedPage, int patientsPerPage, string searchText)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Patients/byImageId/{imageId}/{selectedStatus}/{selectedSortDescription}/{selectedPage}/{patientsPerPage}/{searchText}").Result;
            if (result.IsSuccessStatusCode)
            {
                return result.Content.ReadAsAsync<PatientsToClient>(bsonFormatting).Result;
            }
            else
            {
                throw new Exception($"PatientsToClient GetPatientsByImage( ... ). Status code: {result.StatusCode}");
            }
        }
        public static PatientsToClient GetPatients(string selectedStatus, string selectedSortDescription, int selectedPage, int patientsPerPage, string searchText,int doctorId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Patients/{selectedStatus}/{selectedSortDescription}/{selectedPage}/{patientsPerPage}/{searchText}/{doctorId}").Result;
            if (result.IsSuccessStatusCode)
            {
                return result.Content.ReadAsAsync<PatientsToClient>(bsonFormatting).Result;
            }
            else
            {
                throw new Exception($"PatientsToClient GetPatients( ... doctorId={doctorId} ). Status code: {result.StatusCode}");
            }
        }
        public static PatientsToClient GetDebtors(string selectedSortDescription, int selectedPage, int patientsPerPage, string searchText, int doctorId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Patients/debtors/{selectedSortDescription}/{selectedPage}/{patientsPerPage}/{searchText}/{doctorId}").Result;
            if (result.IsSuccessStatusCode)
            {
                return result.Content.ReadAsAsync<PatientsToClient>(bsonFormatting).Result;
            }
            else
            {
                throw new Exception($"PatientsToClient GetDebtors( ... doctorId={doctorId} ). Status code: {result.StatusCode}");
            }
        }
        public static Doctor Authenticate(LoginModel loginModel)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PostAsync($"Account/login", loginModel, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception("Not authorized");
            }
            Doctor doctor = result.Content.ReadAsAsync<Doctor>(bsonFormatting).Result;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doctor.EncodedJwt);
            return doctor;
        }
        public static Doctor Register(RegisterModel registerModel)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PostAsync($"Account/register", registerModel, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception("Not authorized");
            }
            Doctor doctor = result.Content.ReadAsAsync<Doctor>(bsonFormatting).Result;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doctor.EncodedJwt);
            return doctor;
        }
        internal static void DeleteScheduleRecord(int id)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.DeleteAsync($"Schedule/{id}").Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void DeleteScheduleRecord(id = {id}). Status code: {result.StatusCode}");
            }
        }
        internal static void DeleteStage(int id)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.DeleteAsync($"Stages/{id}").Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void DeleteStage(id = {id}). Status code: {result.StatusCode}");
            }
        }
        internal static void RemoveImageFromStage(int imageId, int stageId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.DeleteAsync($"Stages/removePhotoFromStage/{imageId}/{stageId}").Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void RemoveImageFromStage(imageId = {imageId},stageId = {stageId}). Status code: {result.StatusCode}");
            }
        }
        internal static void DeleteImage(int id)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.DeleteAsync($"Images/{id}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void DeleteImage(id={id}). Status code: {result.StatusCode}");
            }

        }
        internal static void DeletePatient(int id)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.DeleteAsync($"Patients/{id}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void DeletePatients(id={id}). Status code: {result.StatusCode}");
            }

        }
        internal static void PutScheduleRecord(Schedule schedule)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PutAsync($"Schedule", schedule, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void PutScheduleRecord(Schedule schedule). Status code: {result.StatusCode}");
            }
        }
        internal static void PutStage(Stage s)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            StageDTO stageDTO = new StageDTO(s);
            HttpResponseMessage result = httpClient.PutAsync($"Stages", stageDTO, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void PutStage(Stage s). Status code: {result.StatusCode}");
            }
        }
        internal static Schedule PostScheduleRecord(Schedule newRecord)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PostAsync($"Schedule", newRecord, BsonFormatter).Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Schedule PostScheduleRecord(Schedule newRecord). Status code: {result.StatusCode}");
            }
            Schedule record = result.Content.ReadAsAsync<Schedule>(bsonFormatting).Result;
            return record;
        }
        internal static Stage PostStage(Stage stage)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            StageDTO stageDTO = new StageDTO(stage);
            HttpResponseMessage result = httpClient.PostAsync($"Stages", stageDTO, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Stage PostStage(Stage stage). Status code: {result.StatusCode}");
            }
            StageDTO stageFromServer = result.Content.ReadAsAsync<StageDTO>(bsonFormatting).Result;
            return new Stage(stageFromServer);
        }
        internal static Image PostImage(Image image)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PostAsync($"Images", image, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Image PostImage(Image image). Status code: {result.StatusCode}");
            }
            Image imageFromServer = result.Content.ReadAsAsync<Image>(bsonFormatting).Result;
            return imageFromServer;
        }
        internal static Patient PostPatient(Patient patient)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PostAsync($"Patients", patient, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Patient PostPatient(Patient patient). Status code: {result.StatusCode}");
            }
            patient = result.Content.ReadAsAsync<Patient>(bsonFormatting).Result;
            return patient;
        }
        internal static void PutPatient(Patient patient)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PutAsync($"Patients", patient, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Patient PutPatient(Patient patient). Status code: {result.StatusCode}");
            }
        }
        internal static void AddImageToStage(int imageId, int stageId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Stages/addPhotoToStage/{imageId}/{stageId}").Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void AddImageToStage(imageId = {imageId},stageId = {stageId}). Status code: {result.StatusCode}");
            }
        }
        internal static List<StageAsset> GetStageAssets()
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Stages/stageAssets").Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<StageAsset> GetStageAssets(). Status code: {result.StatusCode}");
            }
            List<StageAsset> receivedAssets = result.Content.ReadAsAsync<List<StageAsset>>(bsonFormatting).Result;
            return receivedAssets;
        }
        internal static void PostStageAsset(StageAsset stageAsset)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            stageAsset.Id = 0;
            if(stageAsset.Value == null)
            {
                stageAsset.Value = "";
            }
            HttpResponseMessage result = httpClient.PostAsync($"Stages/stageAsset", stageAsset, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void PostStageAsset(StageAsset stageAsset). Status code: {result.StatusCode}");
            }
            int primaryKey = Convert.ToInt32(result.Content.ReadAsStringAsync().Result);
            stageAsset.Id = primaryKey;
        }
        internal static List<Stage> GetRelatedStagesToSchedule(int scheduleId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Schedule/getRelatedStages/{scheduleId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<Stage> GetRelatedStagesToSchedule(scheduleId={scheduleId}). Status code: {result.StatusCode}");
            }
            List<Stage> receivedStages = result.Content.ReadAsAsync<List<StageDTO>>(bsonFormatting).Result.Select(d=>new Stage(d)).ToList();
            receivedStages.Reverse();
            return receivedStages;
        }
        internal static byte[] GetImageOriginalBytes(int imageId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(octetStreamHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Images/getOriginalBytes/{imageId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"byte[] GetImageOriginalBytes(imageId={imageId}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsByteArrayAsync().Result;
        }
        internal static List<Cabinet> GetCabinets()
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Schedule/getCabinets").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<Cabinet> GetCabinets(). Status code: {result.StatusCode}");
            }
            List<Cabinet> receivedCabinets = result.Content.ReadAsAsync<List<Cabinet>>(bsonFormatting).Result.ToList();
            return receivedCabinets;
        }
        internal static Image[] GetImagesForStage(int stageId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Stages/getPhotosForStage/{stageId}").Result;
            if(result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Image[] GetImagesForStage(stageId={stageId}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<Image[]>(bsonFormatting).Result;
        }
        internal static Doctor[] GetDoctors()
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Doctors/getAll").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Doctor[] GetDoctors(). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<Doctor[]>(bsonFormatting).Result;
        }
        public static List<Stage> GetStages(int patientId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Stages/{patientId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<Stage> GetStages(patientId={patientId}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<List<Stage>>(bsonFormatting).Result;
        }
        public static ImagesToClient GetImages(int selectedPage, int photosPerPage,int doctorId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Images/{selectedPage}/{photosPerPage}/{doctorId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"ImagesToClient GetImages(selectedPage={selectedPage}, photosPerPage={photosPerPage}, doctorId={doctorId}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<ImagesToClient>(bsonFormatting).Result;
        }
        public static void RenameImage(int imageId, string newName)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            StringContent str = new StringContent(newName, Encoding.UTF8, "text/plain");
            HttpResponseMessage result = httpClient.PostAsync($"Images/changeImageName/{imageId}", str).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void RenameImage(imageId={imageId}, newName={newName}). Status code: {result.StatusCode}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="mark">1 means true, 0 means false</param>
        /// <exception cref="Exception"></exception>
        public static void StageMarkSentViaMessager(int stageId, int mark)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Stages/sentViaMessager/{stageId}/{mark}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void StageMarkSentViaMessager(int stageId={stageId}, int mark={mark}). Status code: {result.StatusCode}");
            }
        }
        public static WeekMoneySummaryRequestAnswer GetWeekMoneySummary(int cabinetId, DateTime sunday)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            WeekMoneySummaryRequest r = new WeekMoneySummaryRequest()
            {
                CabinetId = cabinetId,
                AnySunday = sunday
            };
            HttpResponseMessage result = httpClient.PutAsync($"Schedule/weekMoneySummary", r, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"WeekMoneySummaryRequestAnswer GetWeekMoneySummary(cabinetId = {cabinetId}, sunday={sunday}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<WeekMoneySummaryRequestAnswer>(bsonFormatting).Result;
        }

    }
}
