namespace WARSApplication.Data.SharedDataProperties;

public abstract record AuctionRule;

public record EnglishAuctionRule(decimal StartingPrice) : AuctionRule;
public record DutchAuctionRule(decimal StartingPrice) : AuctionRule;
public record FirstTimeSealedBidRule : AuctionRule;