using System;
using System.Threading.Tasks;
using PTEducation.Data.Entities;

namespace PTEducation.Business.Services.AttendanceServices
{
    public interface IAttendanceScheduler
    {
        Task ScheduleAttendanceJobsAsync(Attendance attendance);
        Task RemoveAttendanceJobsAsync(Guid attendanceId);
    }
}
