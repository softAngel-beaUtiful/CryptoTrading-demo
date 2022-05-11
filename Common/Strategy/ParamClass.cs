using Newtonsoft.Json;

namespace TickQuant.Common
{
    public enum EParamType
    {
        //[JsonConverter(typeof(StringEnumConverter)) ]
        BOOL,
        //[JsonConverter(typeof(StringEnumConverter))]
        STRING,
        NUMBER,
        OBJECT,
        ARRAY, 
        NULL        
    }

    public class ParamClass
    {       
        public string Description;
        public string Type;
        public object Value;
        [JsonIgnore]
        public EParamType type { get
            {
                switch (Type)
                    {
                    case "bool": return EParamType.BOOL;
                    case "string": return EParamType.STRING;
                    case "number": return EParamType.NUMBER;
                    case "object": return EParamType.OBJECT;
                    case "array": return EParamType.ARRAY;
                    default:
                    case "null":return EParamType.NULL;
                }; }
            set {
                switch (value)
                {
                    case (EParamType.BOOL):
                        Type = "bool";
                        break;
                    case (EParamType.STRING): 
                        Type = "string";
                        break;
                    case (EParamType.NUMBER):
                        Type = "number";
                        break;
                    case (EParamType.OBJECT):
                        Type = "object";
                        break;
                    case (EParamType.ARRAY):
                        Type = "array";
                        break;
                    case (EParamType.NULL):
                        Type = "null";
                        break;
                };
            } }   //0 - bool, 1-string - 2- number 3-object 4-array 5-null        
    }
    
    
}
