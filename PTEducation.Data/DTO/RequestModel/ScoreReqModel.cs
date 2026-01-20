using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.RequestModel
{
    public class ScoreReqModel
    {
    }

    public class ScoreIdReqModel
    {
        public DateTime TestDateAt { get; set; }
        public Guid ClassId { get; set; }
    }

    public class ScoreCreateReqModel
    {
        public DateTime TestDateAt { get; set; }
        public Guid ClassId { get; set; }
        public string? Shift { get; set; }
        public List<ScoreDetailReqModel> ScoreReqList { get; set; } = new();
    }

    public class ScoreDetailReqModel
    {
        public Guid StudentClassId { get; set; }
        public decimal Score { get; set; }
        public string? Note { get; set; }
    }

    public class ScoreUpdateReqModel
    {
        public Guid Id { get; set; }
        public DateTime TestDateAt { get; set; }
    }

    public class ScoreFilter
    {
        public Guid ClassId { get; set; }
        public DateTime? TestDateAt { get; set; }
        public bool? OrderCreatedAt { get; set; }
    }

    public class ScoreDetailUpdateReqModel
    {
        public Guid Id { get; set; }
        public List<ScoreDetailStudentReqModel> ScoreReqList { get; set; } = new();
    }

    public class ScoreDetailStudentReqModel
    {
        public Guid StudentClassId { get; set; }
        public decimal Score { get; set; }
        public string? Note { get; set; }
    }

    public class ScoreStudentReqModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class ScoreFromDateToDate
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
