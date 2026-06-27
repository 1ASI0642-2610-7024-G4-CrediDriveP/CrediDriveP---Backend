namespace CrediDriveP.API.Helpers;

public static class FinancialEngine
{
    // ──────────────────────────────────────────────────────────────
    // 1. CONVERSIÓN DE TASAS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Convierte TNA a TEA según capitalización.
    /// Capitalización: DAILY=360, MONTHLY=12, QUARTERLY=4, SEMIANNUAL=2, ANNUAL=1
    /// </summary>
    public static decimal TnaToTea(decimal tna, string capitalization)
    {
        int m = capitalization.ToUpper() switch
        {
            "DAILY"      => 360,
            "MONTHLY"    => 12,
            "QUARTERLY"  => 4,
            "SEMIANNUAL" => 2,
            "ANNUAL"     => 1,
            _ => throw new ArgumentException("Capitalización no válida.")
        };

        // TEA = (1 + TNA/m)^m - 1
        double tnaD = (double)tna;
        double tea  = Math.Pow(1 + tnaD / m, m) - 1;
        return (decimal)tea;
    }

    /// <summary>
    /// Convierte TEA a tasa mensual efectiva.
    /// TEM = (1 + TEA)^(1/12) - 1
    /// </summary>
    public static decimal TeaToTem(decimal tea)
    {
        double tem = Math.Pow(1 + (double)tea, 1.0 / 12.0) - 1;
        return (decimal)tem;
    }

    // ──────────────────────────────────────────────────────────────
    // 2. CRONOGRAMA FRANCÉS CON GRACIA Y BALLOON
    // ──────────────────────────────────────────────────────────────

    public class ScheduleRow
    {
        public int    PeriodNumber  { get; set; }
        public string GraceApplied { get; set; } = "NONE";
        public decimal OpeningBalance        { get; set; }
        public decimal Interest              { get; set; }
        public decimal Principal             { get; set; }
        public decimal InsuranceDesgravamen  { get; set; }
        public decimal InsuranceVehicular    { get; set; }
        public decimal Commission            { get; set; }
        public decimal Balloon               { get; set; }
        public decimal TotalPayment          { get; set; }
        public decimal ClosingBalance        { get; set; }
    }

    public class ScheduleParams
    {
        public decimal AmountFinanced        { get; set; }
        public decimal Tea                   { get; set; }  // Ya convertida a TEA
        public int     TermMonths            { get; set; }
        public string  GraceType            { get; set; } = "NONE";
        public int     GraceMonths          { get; set; } = 0;
        public string  PaymentMethod        { get; set; } = "FRENCH";
        public decimal BalloonPct           { get; set; } = 0;
        public decimal VehiclePrice         { get; set; }  // Para seguro vehicular
        public decimal RateDesgravamen      { get; set; }  // Tasa mensual decimal
        public decimal RateVehicular        { get; set; }  // Tasa mensual decimal
        public decimal CommissionMonthly    { get; set; }  // Monto fijo mensual
        public string  InsuranceBaseDesgrv  { get; set; } = "SALDO_INSOLUTO";
        public string  InsuranceBaseVehic   { get; set; } = "VALOR_VEHICULO";
    }

    public static List<ScheduleRow> GenerateSchedule(ScheduleParams p)
    {
        var rows    = new List<ScheduleRow>();
        decimal tem = TeaToTem(p.Tea);
        decimal saldo = p.AmountFinanced;

        // Cuota balloon (si aplica)
        decimal balloon = p.PaymentMethod == "FRENCH_BALLOON"
            ? Math.Round(p.AmountFinanced * p.BalloonPct, 2)
            : 0m;

        // Monto a financiar para calcular cuota francesa
        // (descontando el balloon del principal a amortizar)
        decimal baseFinanciado = p.AmountFinanced - balloon;

        // Cuota francesa fija (sin gracia, sin seguros, sin comisiones)
        decimal cuotaFrancesa = tem == 0 ? baseFinanciado / p.TermMonths
            : Math.Round(
                baseFinanciado * tem /
                (1 - (decimal)Math.Pow(1 + (double)tem,
                    -(p.TermMonths - p.GraceMonths))),
                2);

        for (int i = 1; i <= p.TermMonths; i++)
        {
            var row = new ScheduleRow
            {
                PeriodNumber   = i,
                OpeningBalance = Math.Round(saldo, 2)
            };

            // Interés del período
            row.Interest = Math.Round(saldo * tem, 2);

            // ── Período de gracia ──────────────────────────────────
            bool isGrace = i <= p.GraceMonths;

            if (isGrace && p.GraceType == "TOTAL")
            {
                // Gracia total: no paga interés ni capital
                // El interés se capitaliza al saldo
                row.GraceApplied = "TOTAL";
                row.Principal    = 0;
                row.Balloon      = 0;
                saldo           += row.Interest; // capitaliza interés
                row.Interest     = 0;            // no se cobra
            }
            else if (isGrace && p.GraceType == "PARTIAL")
            {
                // Gracia parcial: paga solo interés, no amortiza capital
                row.GraceApplied = "PARTIAL";
                row.Principal    = 0;
                row.Balloon      = 0;
            }
            else
            {
                // Período normal
                row.GraceApplied = "NONE";

                bool isLastPeriod = i == p.TermMonths;

                if (isLastPeriod && balloon > 0)
                {
                    // Última cuota incluye balloon
                    row.Principal = Math.Round(saldo - balloon - row.Interest, 2);
                    row.Balloon   = balloon;
                    // Ajuste: si el saldo restante es menor
                    if (row.Principal < 0)
                    {
                        row.Balloon  += row.Principal;
                        row.Principal = 0;
                    }
                }
                else
                {
                    row.Principal = Math.Round(cuotaFrancesa - row.Interest, 2);
                    row.Balloon   = 0;

                    // Ajuste última cuota para no quedar saldo residual
                    if (isLastPeriod)
                        row.Principal = Math.Round(saldo, 2);
                }
            }

            // ── Seguros ────────────────────────────────────────────
            row.InsuranceDesgravamen = p.InsuranceBaseDesgrv == "SALDO_INSOLUTO"
                ? Math.Round(saldo * p.RateDesgravamen, 2)
                : Math.Round(p.AmountFinanced * p.RateDesgravamen, 2);

            row.InsuranceVehicular = p.InsuranceBaseVehic == "VALOR_VEHICULO"
                ? Math.Round(p.VehiclePrice * p.RateVehicular, 2)
                : Math.Round(p.AmountFinanced * p.RateVehicular, 2);

            // ── Comisión ───────────────────────────────────────────
            row.Commission = Math.Round(p.CommissionMonthly, 2);

            // ── Total ──────────────────────────────────────────────
            row.TotalPayment = row.Interest
                             + row.Principal
                             + row.InsuranceDesgravamen
                             + row.InsuranceVehicular
                             + row.Commission
                             + row.Balloon;

            // ── Saldo cierre ───────────────────────────────────────
            saldo -= (row.Principal + row.Balloon);
            row.ClosingBalance = Math.Round(saldo, 2);

            rows.Add(row);
        }

        return rows;
    }

    // ──────────────────────────────────────────────────────────────
    // 3. VAN (desde perspectiva del deudor)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// VAN del deudor: flujo inicial positivo (recibe el préstamo)
    /// y pagos negativos (cuotas totales).
    /// COK anual → convertir a mensual para descontar.
    /// </summary>
    public static decimal CalculateVan(
        decimal amountFinanced,
        List<ScheduleRow> schedule,
        decimal cokAnnual)
    {
        double cokMensual = Math.Pow(1 + (double)cokAnnual, 1.0 / 12.0) - 1;
        double van        = (double)amountFinanced; // flujo 0: recibe el préstamo

        for (int t = 0; t < schedule.Count; t++)
        {
            double flujo    = -(double)schedule[t].TotalPayment;
            double factor   = Math.Pow(1 + cokMensual, t + 1);
            van            += flujo / factor;
        }

        return (decimal)Math.Round(van, 4);
    }

    // ──────────────────────────────────────────────────────────────
    // 4. TIR (Newton-Raphson mensual)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// TIR mensual del préstamo desde perspectiva del deudor.
    /// Flujo 0 = +amountFinanced, flujos 1..n = -totalPayment
    /// </summary>
    public static decimal CalculateTirMonthly(
        decimal amountFinanced,
        List<ScheduleRow> schedule,
        int maxIterations = 1000,
        double tolerance  = 1e-8)
    {
        double[] flows = new double[schedule.Count + 1];
        flows[0] = (double)amountFinanced;
        for (int i = 0; i < schedule.Count; i++)
            flows[i + 1] = -(double)schedule[i].TotalPayment;

        double r = 0.01; // estimación inicial 1% mensual

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double npv  = 0;
            double dnpv = 0;

            for (int t = 0; t < flows.Length; t++)
            {
                double disc  = Math.Pow(1 + r, t);
                npv         += flows[t] / disc;
                if (t > 0)
                    dnpv    -= t * flows[t] / Math.Pow(1 + r, t + 1);
            }

            double rNew = r - npv / dnpv;

            if (Math.Abs(rNew - r) < tolerance)
                return (decimal)Math.Round(rNew, 8);

            r = rNew;
        }

        return (decimal)Math.Round(r, 8);
    }

    /// <summary>TIR anual desde TIR mensual: (1+TIRm)^12 - 1</summary>
    public static decimal TirMonthlyToAnnual(decimal tirMonthly)
    {
        double annual = Math.Pow(1 + (double)tirMonthly, 12) - 1;
        return (decimal)Math.Round(annual, 8);
    }

    // ──────────────────────────────────────────────────────────────
    // 5. TCEA
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// TCEA = TIR anual efectiva del costo total del crédito
    /// (incluye seguros y comisiones en el flujo).
    /// En este modelo, la TIR ya incluye todos los costos
    /// porque TotalPayment los contiene → TCEA = TIR anual.
    /// </summary>
    public static decimal CalculateTcea(decimal tirAnnual) => tirAnnual;
}