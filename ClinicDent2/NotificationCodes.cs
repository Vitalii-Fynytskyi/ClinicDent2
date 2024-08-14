namespace ClinicDent2
{
    public static class NotificationCodes
    {
        /// <summary>
        /// int recordId is param
        /// </summary>
        public const int ScheduleRecordDeletedIdOnly = 1;
        /// <summary>
        /// Schedule s is param
        /// </summary>
        public const int ScheduleRecordAdded = 2;
        /// <summary>
        /// Schedule s is param
        /// </summary>
        public const int ScheduleRecordUpdated = 3;

        /// <summary>
        /// int patientId is param
        /// </summary>
        public const int PatientStagesUpdated = 4;

        /// <summary>
        /// string updatedStagesContent is param (values separated by commas)
        /// </summary>
        public const int ScheduleStagesPaymentUpdated = 5;
        public const int ScheduleCabinetCommentUpdated = 6;
        public const int ScheduleRecordCommentUpdated = 7;
        public const int ScheduleRecordStateUpdated = 8;
        public const int ScheduleRecordDeleted = 9;

    }
}