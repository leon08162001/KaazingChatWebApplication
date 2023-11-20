using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KaazingChatApi.Controllers
{
    /// <summary>
    /// 上下班打卡所使用的web api
    /// </summary>
    //[Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class PunchController : ControllerBase
    {
        /// <summary>
        /// 取得某人ID,特定日期的打卡紀錄
        /// </summary>
        /// <param name="Input">PunchInput類別</param>
        /// <returns></returns>
        [HttpPost]
        //[Route("GetPunch")]
        public PunchRecord GetPunch(PunchInput Input)
        {
            PunchRecord PR = new PunchRecord();
            var str = HttpContext.Session.GetString(Input.ID + "_" + Input.QueryDate);
            if (str == null)
            {
                PR.ID= Input.ID;
                PR.QueryDate = Input.QueryDate;
            }
            else
            {
                PR = JsonConvert.DeserializeObject<PunchRecord>(str);
            }
            return PR;
        }
        /// <summary>
        /// 寫入某人ID,特定日期的上班或下班打卡紀錄
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        [HttpPost]
        //[Route("SetPunch")]
        public PunchRecord SetPunch(PunchInput Input)
        {
            PunchRecord PR = new PunchRecord();
            var str = HttpContext.Session.GetString(Input.ID + "_" + Input.QueryDate);
            if (str == null)
            {
                PR.ID = Input.ID;
                PR.QueryDate = Input.QueryDate;
            }
            else
            {
                PR = JsonConvert.DeserializeObject<PunchRecord>(str);
            }
            if (Input.PunchType == PunchType.上班)
            {
                PR.PunchOnTime = DateTime.Now;
            }
            else
            {
                PR.PunchOffTime = DateTime.Now;
            }
            var json = JsonConvert.SerializeObject(PR);
            HttpContext.Session.SetString(Input.ID + "_" + Input.QueryDate, json);
            return PR;
        }
    }
    /// <summary>
    /// 打卡查詢或建立輸入參數類別
    /// </summary>
    public class PunchInput
    {
        /// <summary>
        /// 人員ID
        /// </summary>
        public string ID
        {
            get;
            set;

        }
        /// <summary>
        /// 打卡日期(yyyy-mm-dd)
        /// </summary>
        public string QueryDate
        {
            get;
            set;
        }
        /// <summary>
        /// 打卡類型-0:上班;1:下班(不影響回傳資料,因都會回傳該員所選日期的上下班打卡紀錄)
        /// </summary>
        public PunchType PunchType
        {
            get;
            set;
        }
    }
    /// <summary>
    /// 打卡紀錄類別
    /// </summary>
    public class PunchRecord
    {
        /// <summary>
        /// 人員ID
        /// </summary>
        public string ID
        {
            get;
            set;

        }
        /// <summary>
        /// 打卡日期(yyyy-mm-dd)
        /// </summary>
        public string QueryDate
        {
            get;
            set;
        }
        /// <summary>
        /// 上班打卡時間
        /// </summary>
        public DateTime PunchOnTime
        {
            get;
            set;
        }
        /// <summary>
        /// 下班打卡時間
        /// </summary>
        public DateTime PunchOffTime
        {
            get;
            set;
        }
    }
    /// <summary>
    /// 打卡類型(0:上班;1:下班)
    /// </summary>
    public enum PunchType
    {
        上班=0,
        下班=1
    }
}
