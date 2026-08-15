using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Outgoing;
public record struct BuyerAuctionInfo(
        AuctionId AuctionId, string Description, UserId SellerId, 
        DateTimeOffset StartTime, DateTimeOffset EndTime, AuctionStatus Status, AuctionTypeSpecificRules InfoData 
    );

public abstract record TypeSpecificAuctionInfo; 

public sealed record DutchAuctionInfo(Price CurrentPrice) : TypeSpecificAuctionInfo;
public sealed record EnglishAuctionInfo(Price CurrentBid) : TypeSpecificAuctionInfo;
public sealed record FirstTimeSealedBidInfo : TypeSpecificAuctionInfo;