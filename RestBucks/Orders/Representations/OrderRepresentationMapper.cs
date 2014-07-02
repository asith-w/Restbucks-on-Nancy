namespace RestBucks.Orders.Representations
{
  using System.Collections.Generic;
  using System.Linq;
  using Domain;
  using Infrastructure;
  using Infrastructure.Linking;
  using Infrastructure.Resources;
  using Nancy.Routing;

  public static class OrderRepresentationMapper
  {
    public static OrderRepresentation Map(Order order, string baseAddress, IRouteCache routes)
    {
      return new OrderRepresentation(order)
      {
        Links = GetLinks(order, baseAddress, routes).ToList()
      };
    }

    private static IEnumerable<Link> GetLinks(Order order, string baseAddress, IRouteCache routes)
    {
      var allRoutes = routes.SelectMany(pair => pair.Value.Select(tuple => tuple.Item2)).ToList();
      var baseUri = new UriSegment(baseAddress);
      var linker = new ResourceLinker(baseAddress);

      var get = new Link(linker.BuildUriString(string.Empty,
        allRoutes.Single(r => r.Name == "ReadOrder").Path,
        new {orderId = order.Id}),
        baseUri + "docs/order-get.htm",
        MediaTypes.Default);

      var update = new Link(linker.BuildUriString(string.Empty,
        allRoutes.Single(r => r.Name == "UpdateOrder").Path,
        new { orderId = order.Id}),
        baseUri + "docs/order-update.htm",
        MediaTypes.Default);

      var cancel = new Link(linker.BuildUriString(string.Empty,
        allRoutes.Single(r => r.Name == "CancelOrder").Path,
        new { orderId = order.Id}),
        baseUri + "docs/order-cancel.htm",
        MediaTypes.Default);

      var pay = new Link(linker.BuildUriString(string.Empty,
        allRoutes.Single(r => r.Name == "PayOrder").Path,
        new {orderId = order.Id}),
        baseUri + "docs/order-pay.htm",
        MediaTypes.Default);

      var receipt = new Link(linker.BuildUriString(OrderResourceModule.Path,
        OrderResourceModule.ReceiptPath,
        new {orderId = order.Id}),
        baseUri + "docs/receipt-coffee.htm",
        MediaTypes.Default);

      switch (order.Status)
      {
        case OrderStatus.Unpaid:
          yield return get;
          yield return update;
          yield return cancel;
          yield return pay;
          break;
        case OrderStatus.Paid:
        case OrderStatus.Delivered:
          yield return get;
          break;
        case OrderStatus.Ready:
          yield return receipt;
          break;
        case OrderStatus.Canceled:
          yield break;
        default:
          yield break;
      }
    }
  }
}