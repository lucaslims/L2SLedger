using System.Diagnostics.Metrics;

namespace L2SLedger.Infrastructure.Observability;

/// <summary>
/// Métricas customizadas da aplicação L2SLedger.
/// Conforme ADR-006: Métricas mínimas para observabilidade.
/// </summary>
public class ApplicationMetrics
{
    public const string MeterName = "L2SLedger.API";

    private readonly Counter<long> _authOperationsCounter;
    private readonly Counter<long> _transactionOperationsCounter;
    private readonly Counter<long> _exportOperationsCounter;
    private readonly Histogram<double> _exportDurationHistogram;

    public ApplicationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _authOperationsCounter = meter.CreateCounter<long>(
            "l2sledger_auth_operations_total",
            description: "Total de operações de autenticação");

        _transactionOperationsCounter = meter.CreateCounter<long>(
            "l2sledger_transaction_operations_total",
            description: "Total de operações com transações");

        _exportOperationsCounter = meter.CreateCounter<long>(
            "l2sledger_export_operations_total",
            description: "Total de operações de exportação");

        _exportDurationHistogram = meter.CreateHistogram<double>(
            "l2sledger_export_duration_seconds",
            unit: "s",
            description: "Duração das operações de exportação");
    }

    public void RecordAuthOperation(string operation, string result)
    {
        _authOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("result", result));
    }

    public void RecordTransactionOperation(string operation)
    {
        _transactionOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("operation", operation));
    }

    public void RecordExportOperation(string format, string status)
    {
        _exportOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("format", format),
            new KeyValuePair<string, object?>("status", status));
    }

    public void RecordExportDuration(double durationSeconds, string format)
    {
        _exportDurationHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("format", format));
    }
}
