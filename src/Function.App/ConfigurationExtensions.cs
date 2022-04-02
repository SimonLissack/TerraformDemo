using Microsoft.Extensions.Configuration;

namespace lissack.io.function;

public static class ConfigurationExtensions {
    static string _queueNameString = "Queue:Name";
    static string _tableNameString = "Table:Name";

    public static string QueueName(this IConfiguration config){
        return config.GetValue<string>(_queueNameString);
    }

    public static string TableName(this IConfiguration config){
        return config.GetValue<string>(_tableNameString);
    }
}
