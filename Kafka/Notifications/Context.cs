namespace Bankly.Sdk.Kafka.Notifications
{
    public enum Context
    {
        ///
        /// Agrupa todos os eventos relacionados à upload e análise de documents.
        ///
        Document,

        ///
        /// Agrupa todos os eventos relacionados à gestão de dispositivos.
        ///
        Device,

        ///
        /// Agrupa todos os eventos relacionados à onboarding, offboarding de clientes pessoas jurídica.
        ///
        Business,

        ///
        /// Agrupa todos os eventos relacionados à onboarding, offboarding de clientes pessoa física.
        ///
        Customer,

        ///
        /// Agrupa todos os eventos relacionados à gestão de contas (onboarding, offboarding, pockets, etc).
        ///
        Account,

        ///
        /// Agrupa todos os eventos relacionados à gestão de cartões (pré e pós) e programas.
        ///
        Card,

        ///
        /// Agrupa todos os eventos de autorização de cartões (pré e pós), base 1 e base 2, chargeback, etc...
        ///
        Authorization,

        ///
        /// Agrupa todos os eventos relacionados à pagamentos de contas.
        ///
        Payment,

        ///
        /// Agrupa todos os eventos relacionados à cash-in e cash-out, refunds e qrcodes Pix.
        ///
        Pix,

        ///
        /// Agrupa todos os eventos relacionados à Gestão de Chaves e Reinvindicações de Posse e Portabilidade de chaves Pix.
        ///
        Dict,

        ///
        /// Agrupa todos os eventos relacionados à Boletos (emissão, cancelamento, compensação, etc).
        ///
        Boleto,

        ///
        /// Agrupa todos os eventos relacionados à cash-in e cash-out, refunds via TED.
        ///
        Ted,

        ///
        /// Agrupa todos os eventos relacionados à cash-in e cash-out, ordens de compra e venda de Criptomoedas.
        ///
        Crypto,

        ///
        /// Agrupa todos os eventos relacionados à crédito.
        ///
        Credit,

        ///
        /// Agrupa todos os eventos relacionados à faturas.
        ///
        Invoice,

        ///
        /// Agrupa todos os eventos relacionados à gestão de parceiros.
        ///
        Partner
    }
}
