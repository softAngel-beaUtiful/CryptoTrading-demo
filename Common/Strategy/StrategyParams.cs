using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace TickQuant.Common
{
    public class StrategyParams:BasicParams
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserID { get ; set ; }       
        /// <summary>
        /// 重复模式
        /// </summary>
        public ERepeatMode RepeatMode { get; set; }       
        /// <summary>
        /// 起始数量值类型
        /// </summary>
        public EValueMode QuantValueMode { get; set; }
        /// <summary>
        /// 起始数量值
        /// </summary>
        public decimal QuantValue { get; set; }                      
        public ERunMode RunMode { get; set; }
       
        /// <summary>
        /// 特殊参数列表
        /// </summary>
        public Dictionary<string, ParamClass> SpecialParams { get; } = new Dictionary<string, ParamClass>();        //KEY: 参数名

        public new void WriteToJsonFile(string path=null)
        {
            string filepath = path is null ? "CustomStrategies/"+StrategyName+"/"+StrategyName + ".json" : path;
            StreamWriter writer = new StreamWriter(filepath);           
            writer.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            writer.Close();
        }

        public new StrategyParams ReadFromJsonFile(string strategyName)
        {
            StreamReader reader = new StreamReader(strategyName + ".json");
            var r = reader.ReadToEnd();
            reader.Close();
            return JsonConvert.DeserializeObject<StrategyParams>(r);
        }
    }
    public class BasicParams: INotifyPropertyChanged
    {      
        /// <summary>
        /// 策略ID
        /// </summary>
        public string StrategyID { get; set; }
        /// <summary>
        /// 策略名称
        /// </summary>
        /// 
        private string strategyname;
        public string StrategyName
        {
            get { return strategyname; }
            set
            {
                if (strategyname != value)
                {
                    strategyname = value;
                    if (!(PropertyChanged is null))
                        PropertyChanged(this, new PropertyChangedEventArgs(value));
                }
            }
        }
        /// <summary>
        /// 策略描述
        /// </summary>
        public string Description { get; set; }        
        /// <summary>
        /// 多空操作方向
        /// </summary>
        public EOperationMode OperationMode { get; set; }       
        /// <summary>
        /// 止损目标值类型
        /// </summary>
        public EValueMode StopLossValueMode { get; set; }
        /// <summary>
        /// 止损目标值
        /// </summary>
        public decimal StopLossValue { get; set; }
        /// <summary>
        /// 止盈目标值类型
        /// </summary>
        public EValueMode StopProfitValueMode { get; set; }
        /// <summary>
        /// 止盈目标值
        /// </summary>
        public decimal StopProfitValue { get; set; }    
        public bool TickOrBar { get; set; } 
        public int BarsLookBack { get; set; }
        /// <summary>
        /// 合约列表
        /// </summary>
        public List<SerieType> Instruments { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void WriteToJsonFile(string path = null)
        {
            string filepath = path is null ? StrategyName + ".json" : path;
            StreamWriter writer = new StreamWriter(filepath);
            writer.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            writer.Close();
        }

        public BasicParams ReadFromJsonFile(string strategyName)
        {
            StreamReader reader = new StreamReader(strategyName + ".json");
            var r = reader.ReadToEnd();
            reader.Close();
            return JsonConvert.DeserializeObject<BasicParams>(r);
        }
    }
}

