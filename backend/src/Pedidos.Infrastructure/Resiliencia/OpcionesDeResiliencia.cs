namespace Pedidos.Infrastructure.Resiliencia;

public sealed class OpcionesDeResiliencia
{
    public const string SeccionDeConfiguracion = "Resiliencia";

    public int ReintentosMaximos { get; set; } = 3;

    public int RetardoBaseEnMilisegundos { get; set; } = 200;

    public int TimeoutDeLecturaEnSegundos { get; set; } = 10;

    public int TimeoutDeEscrituraEnSegundos { get; set; } = 15;

    public double ProporcionDeFallosParaAbrir { get; set; } = 0.5;

    public int MuestrasMinimasParaAbrir { get; set; } = 8;

    public int VentanaDeMuestreoEnSegundos { get; set; } = 30;

    public int DuracionDeAperturaEnSegundos { get; set; } = 15;
}
