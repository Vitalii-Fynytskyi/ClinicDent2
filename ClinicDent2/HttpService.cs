using ClinicDent2.Exceptions;
using ClinicDent2.Model;
using ClinicDent2.RequestAnswers;
using ClinicDent2.Requests;
using ClinicDent2.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

namespace ClinicDent2
{
    public static class HttpService
    {
        public static HttpClient httpClient;
        static MediaTypeFormatter[] bsonFormatting;
        static MediaTypeFormatter BsonFormatter;
        static MediaTypeWithQualityHeaderValue bsonHeaderValue;
        static MediaTypeWithQualityHeaderValue octetStreamHeaderValue;
        static HttpService()
        {
            
            BsonFormatter = new BsonMediaTypeFormatter();
            bsonFormatting = new MediaTypeFormatter[] { BsonFormatter };
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="patientIdToSet"></param>
        /// <param name="curePlanToSet"></param>
        /// <param name="lastModifiedDateTime"></param>
        /// <returns>new patient's lastmodified datetime from server</returns>
        /// <exception cref="ConflictException"></exception>
        /// <exception cref="Exception"></exception>
        internal static string PutPatientCurePlan(int patientIdToSet, string curePlanToSet, string lastModifiedDateTime)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            ChangeCurePlanRequest r = new ChangeCurePlanRequest()
            {
                CurePlan = curePlanToSet,
                PatientId = patientIdToSet,
                LastModifiedDateTime=lastModifiedDateTime
            };
            HttpResponseMessage result = httpClient.PutAsync($"Patients/changeCurePlan", r, BsonFormatter).Result;
            if(result.IsSuccessStatusCode == false)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new ConflictException(result.Content.ReadAsAsync<ServerErrorMessage>(bsonFormatting).Result.errorMessage);
                }
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(result.Content.ReadAsAsync<ServerErrorMessage>(bsonFormatting).Result.errorMessage);
                }
                throw new Exception($"void PutPatientCurePlan(patientIdToSet={patientIdToSet},curePlanToSet={curePlanToSet}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsStringAsync().Result;
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
        internal static List<Stage> GetManyStages(List<int> stagesId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);

            string body = String.Join(",", stagesId);
            StringContent str = new StringContent(String.Join(",", stagesId), Encoding.UTF8, "text/plain");
            HttpResponseMessage result = httpClient.PostAsync($"Stages/getMany", str).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<Stage> GetManyStages(stagesId = {body}). Status code: {result.StatusCode}");
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
        public static DebtPatientsToClient GetDebtors(string selectedSortDescription, int selectedPage, int patientsPerPage, string searchText, int doctorId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Patients/debtors/{selectedSortDescription}/{selectedPage}/{patientsPerPage}/{searchText}/{doctorId}").Result;
            if (result.IsSuccessStatusCode)
            {
                return result.Content.ReadAsAsync<DebtPatientsToClient>(bsonFormatting).Result;
            }
            else
            {
                throw new Exception($"DebtPatientsToClient GetDebtors( ... doctorId={doctorId} ). Status code: {result.StatusCode}");
            }
        }
        public static string[] GetTenantList()
        {
            httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "ServerAddress"), TimeSpan.FromSeconds(10));
            HttpResponseMessage result = null;
            try
            {
                result = httpClient.GetAsync($"Account/tenantNames").Result;
            }
            catch (Exception)
            {
                httpClient.Dispose();
                httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "LanServerAddress"), TimeSpan.FromSeconds(10));
                result = httpClient.GetAsync($"Account/tenantNames").Result;
            }
            if (result.IsSuccessStatusCode == false)
            {
                httpClient.Dispose();
                throw new Exception("Can't retrieve tenant list");
            }
            httpClient.Dispose();
            return result.Content.ReadAsAsync<string[]>(bsonFormatting).Result;
        }
        public static string GetApiVersion()
        {
            httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "ServerAddress"), TimeSpan.FromSeconds(10));
            HttpResponseMessage result = null;
            try
            {
                result = httpClient.GetAsync($"Account/apiVersion").Result;
            }
            catch (Exception)
            {
                httpClient.Dispose();
                httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "LanServerAddress"), TimeSpan.FromSeconds(10));
                result = httpClient.GetAsync($"Account/apiVersion").Result;
            }
            if (result.IsSuccessStatusCode == false)
            {
                httpClient.Dispose();
                throw new Exception("Can't retrieve server version");
            }
            httpClient.Dispose();
            return result.Content.ReadAsStringAsync().Result;
        }
        public static Doctor Authenticate(LoginModel loginModel)
        {

            httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "ServerAddress"), TimeSpan.FromSeconds(10));
            HttpResponseMessage result = null;
            try
            {
                result = httpClient.PostAsync($"Account/login", loginModel, BsonFormatter).Result;
            }
            catch(Exception)
            {
                httpClient.Dispose();
                httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "LanServerAddress"), TimeSpan.FromSeconds(10));
                result = httpClient.PostAsync($"Account/login", loginModel, BsonFormatter).Result;
            }
            if (result.IsSuccessStatusCode == false)
            {
                httpClient.Dispose();
                return null;
            }
            Doctor doctor = result.Content.ReadAsAsync<Doctor>(bsonFormatting).Result;
            httpClient.Dispose();
            httpClient = CreateHttpClient(httpClient.BaseAddress.ToString());
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doctor.EncodedJwt);
            return doctor;
        }
        public static Doctor Register(RegisterModel registerModel)
        {
            httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "ServerAddress"), TimeSpan.FromSeconds(10));
            HttpResponseMessage result = null;
            try
            {
                result = httpClient.PostAsync($"Account/register", registerModel, BsonFormatter).Result;
            }
            catch (Exception)
            {
                httpClient.Dispose();
                httpClient = CreateHttpClient(IniService.GetPrivateString("Settings", "LanServerAddress"), TimeSpan.FromSeconds(10));
                result = httpClient.PostAsync($"Account/register", registerModel, BsonFormatter).Result;
            }
            if (result.IsSuccessStatusCode == false)
            {
                httpClient.Dispose();
                throw new Exception("Not authorized");
            }
            Doctor doctor = result.Content.ReadAsAsync<Doctor>(bsonFormatting).Result;
            httpClient.Dispose();
            httpClient = CreateHttpClient(httpClient.BaseAddress.ToString());
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doctor.EncodedJwt);
            return doctor;
        }
        public static HttpClient CreateHttpClient(string serverAddress, TimeSpan? timeout = null)
        {
            HttpClient newHttpClient = new HttpClient();
            newHttpClient.DefaultRequestHeaders.Accept.Clear();
            newHttpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            newHttpClient.BaseAddress=new Uri(serverAddress);
            if(timeout != null)
            {
                newHttpClient.Timeout = timeout.Value;
            }
            return newHttpClient;
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
        internal static void PutStages(List<Stage> s)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            List<StageDTO> stageDTO = s.Select(l => new StageDTO(l)).ToList();
            HttpResponseMessage result = httpClient.PutAsync($"Stages/putMany", stageDTO, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void PutStages(List<Stage> s). Status code: {result.StatusCode}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="ConflictException">returns ids of conflict stages separated by comma.Example "1,2,3,4"</exception>
        /// <exception cref="Exception"></exception>
        internal static PutStagesRequestAnswer PutStages(List<StageDTO> s)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            PutStagesRequest putStagesRequest = new PutStagesRequest
            {
                stageDTO = s
            };
            HttpResponseMessage result = httpClient.PutAsync($"Stages/putMany", putStagesRequest, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    PutStagesRequestAnswer putStagesRequestAnswer = result.Content.ReadAsAsync<PutStagesRequestAnswer>(bsonFormatting).Result;
                    throw new ConflictException("", putStagesRequestAnswer);
                }
                throw new Exception($"void PutStages(List<StageDTO> s). Status code: {result.StatusCode}");
            }
            else
            {
                PutStagesRequestAnswer putStagesRequestAnswer = result.Content.ReadAsAsync<PutStagesRequestAnswer>(bsonFormatting).Result;
                return putStagesRequestAnswer;
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
        internal static string PutPatient(Patient patient)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PutAsync($"Patients", patient, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new ConflictException(result.Content.ReadAsAsync<ServerErrorMessage>(bsonFormatting).Result.errorMessage);
                }
                throw new Exception($"Patient PutPatient(Patient patient). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsStringAsync().Result;
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
        internal static Cabinet[] GetCabinets()
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Schedule/getCabinets").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"Cabinet[] GetCabinets(). Status code: {result.StatusCode}");
            }
            Cabinet[] receivedCabinets = result.Content.ReadAsAsync<Cabinet[]>(bsonFormatting).Result.ToArray();
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
        public static List<Schedule> GetPatientFutureAppointments(int patientId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Schedule/getPatientFutureAppointments/{patientId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($" List<Schedule> GetPatientFutureAppointments(int patientId = {patientId}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<List<Schedule>>(bsonFormatting).Result;
        }
        public static ImagesToClient GetImages(int selectedPage, int photosPerPage,int doctorId, ImageType imageType)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Images/{selectedPage}/{photosPerPage}/{doctorId}/{imageType}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"ImagesToClient GetImages(selectedPage={selectedPage}, photosPerPage={photosPerPage}, doctorId={doctorId}, imageType={imageType}). Status code: {result.StatusCode}");
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
        public static WeekMoneySummaryRequestAnswer GetWeekMoneySummary(DateTime sunday)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            WeekMoneySummaryRequest r = new WeekMoneySummaryRequest()
            {
                AnySunday = sunday
            };
            HttpResponseMessage result = httpClient.PutAsync($"Schedule/weekMoneySummary", r, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"WeekMoneySummaryRequestAnswer GetWeekMoneySummary(sunday={sunday}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<WeekMoneySummaryRequestAnswer>(bsonFormatting).Result;
        }

        public static int GetFutureWorkingMinutes(int cabinetId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            HttpResponseMessage result = httpClient.GetAsync($"Statistics/futureWorkingMinutes/{cabinetId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"int GetFutureWorkingHours(int cabinetId = {cabinetId}). Status code: {result.StatusCode}");
            }
            return Convert.ToInt32(result.Content.ReadAsStringAsync().Result);
        }

        public static int GetFutureUniquePatients(int cabinetId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            HttpResponseMessage result = httpClient.GetAsync($"Statistics/futureUniquePatientsAmount/{cabinetId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"int  GetFutureUniquePatients(int cabinetId = {cabinetId}). Status code: {result.StatusCode}");
            }
            return Convert.ToInt32(result.Content.ReadAsStringAsync().Result);
        }
        public static ToothUnderObservation GetToothUnderObservation(int toothObservationId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync($"Observations/tooth/{toothObservationId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"ToothUnderObservation GetToothUnderObservation(toothObservationId = {toothObservationId}). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<ToothUnderObservation>(bsonFormatting).Result;
        }
        public static List<ToothUnderObservation> GetAllToothUnderObservation()
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.GetAsync("Observations/allTooth").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"List<ToothUnderObservation> GetAllToothUnderObservation(). Status code: {result.StatusCode}");
            }
            return result.Content.ReadAsAsync<List<ToothUnderObservation>>(bsonFormatting).Result;
        }
        internal static int PostToothUnderObservation(ToothUnderObservation newRecord)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            HttpResponseMessage result = httpClient.PostAsync($"Observations/tooth", newRecord, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"int PostToothUnderObservation(ToothUnderObservation newRecord). Status code: {result.StatusCode}");
            }
            return Convert.ToInt32(result.Content.ReadAsStringAsync().Result);
        }
        internal static void PutToothUnderObservation(ToothUnderObservation updatedRecord)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.PutAsync($"Observations/tooth", updatedRecord, BsonFormatter).Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"void PutToothUnderObservation(ToothUnderObservation updatedRecord). Status code: {result.StatusCode}");
            }
        }
        public static void DeleteToothUnderObservation(int toothObservationId)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(bsonHeaderValue);
            HttpResponseMessage result = httpClient.DeleteAsync($"Observations/tooth/{toothObservationId}").Result;
            if (result.IsSuccessStatusCode == false)
            {
                throw new Exception($"ToothUnderObservation DeleteToothUnderObservation(toothObservationId = {toothObservationId}). Status code: {result.StatusCode}");
            }
        }
    }
}
