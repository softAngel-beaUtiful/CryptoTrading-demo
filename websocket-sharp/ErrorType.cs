namespace WebSocketSharp
{
    public enum MessageType
    {
        ErrorProtocol,
        ErrorSend,
        ConnectionLost,
        ConnectionNotOpen,
        Connecting,
        Accepting,
        UnableConnected,
        UnableClosed,
        ErrorOnReceivingMessage,
        NullOrUndefined,
        Callback,
        InterruptedSend,
        ErrorOnHandlingOnOpen,
        Reconnected,
        ErrorWriteFile,
    }
    /*public class ErrorType
    {
        public MessageType messageType;
        public string message;
    }*/
}