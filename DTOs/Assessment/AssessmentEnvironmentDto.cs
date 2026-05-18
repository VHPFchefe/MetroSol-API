namespace MetroSol.API.DTOs.Assessment;

public class AssessmentEnvironmentDto
{
    public double? InitialTemperature { get; set; }
    public double? MiddleTemperature { get; set; }
    public double? FinalTemperature { get; set; }

    public double? InitialHumidity { get; set; }
    public double? MiddleHumidity { get; set; }
    public double? FinalHumidity { get; set; }

    public double? InitialPressure { get; set; }
    public double? MiddlePressure { get; set; }
    public double? FinalPressure { get; set; }
}
