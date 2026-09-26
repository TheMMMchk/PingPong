public class HealthPresenter
{
    private HealthModel _healthModel;



    private HealthView _healthView;
    public HealthPresenter(HealthModel healthModel)
    {
        _healthModel = healthModel;
    }
    public HealthPresenter(HealthModel healthModel, HealthView healthView)
    {
        _healthModel = healthModel;
        _healthView = healthView;
    }

    public void TakeDamage(int damage)
    {
        _healthModel.TakeDamage(damage);
        UpdateHpText();
    }

    public void UpdateHpText() => _healthView?.UpdateHealthText(_healthModel.Hp);
}
