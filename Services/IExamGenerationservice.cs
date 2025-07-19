using System;
using System.Threading.Tasks;

namespace ExamTimeTable.Services
{
    public interface IExamGenerationService
    {
        Task GenerateExamTimetableAsync(DateTime examStartDate);
    }
}
