# Storage Upgrade

Allows to upgrade size of containers

## Features

- Upgrade container rows
- Upgrade container column (off by default)
- refund upgrade cost on destroy

## Notes

Base game handles items within container outside of inventory size rows, but not column ( which is why upgrade container column is off by default ); 
Thus, if you allow to upgrade container columns, do not remove plugin or else the UI for that container will break.

## Cost Calculation

- `Full`: the cost to upgrade is determined by the total cost of the piece
- `Half`: the cost to upgrade is determined by half the cost of the piece
- `Percentage`: the cost to upgrade is determined by the added slots difference from the base size as a percentage of the cost of the piece

Percentage example:
```yml
Wooden chest: 
  original cost: 10x Wood
  base size: 5 columns x 2 rows ( 10 slots )
  added row: 1
  new size: 5 columns x 3 rows
  slot difference: 5 (50%)
  cost: 5x Wood
```
Let's say you upgraded the column by 1 and now upgrade row by 1:
```yml
Wooden chest:
  original cost: 10x Wood
  base size: 5 columns x 2 rows ( 10 slots )
  previously added column: 1
  added row: 1
  new size: 6 columns x 3 rows
  slot difference: 6 (60%)
  cost: 6x Wood
```

## Custom Upgrade Map

If you want to define custom cost to upgrade, find config entry `Custom Upgrade Map`

```yml
## Define upgrade costs, [prefabId]:[itemId]*[amount],...;... [Synced with Server]
# Setting type: String
# Default value:
Custom Upgrade Map = piece_chest_wood:Wood*5,Iron*1
```

This example shows how to define upgrade cost of `piece_chest_wood`, for a cost of 5x Wood and 1x Iron

- Multiple prefab entries are separated by `;`
- Multiple item entries are separated by `,`

This will change the base cost of the upgrade ( the default is the cost to build the piece container ), 
then the actual cost will be calculated based on desired configured option (`Full`, `Half`, `Percentage`)

## Need a dedicated server ?

<a href="https://www.survivalservers.com/?ref=rustymods">
<img src="https://www.survivalservers.com/themes/epona/images/logo.png">
</a> 

Use this link: https://www.survivalservers.com/?ref=rustymods , I'll get a 20% commission. Thanks

## Donations
If you enjoy this mod and want to support me:
[PayPal](https://paypal.me/mpei)

<span>
<img src="https://i.imgur.com/rbNygUc.png" alt="" width="150">
<img src="https://i.imgur.com/VZfZR0k.png" alt="https://www.buymeacoffee.com/peimalcolm2" width="150">
</span>