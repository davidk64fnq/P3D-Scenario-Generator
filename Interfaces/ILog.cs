namespace P3D_Scenario_Generator.Interfaces
{
    public interface ILog
    {
        void Info(string message);
        void Error(string message, Exception ex = null);
        void Warning(string message);
    }
}