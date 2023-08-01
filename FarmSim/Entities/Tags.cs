namespace FarmSim.Entities;

enum Tags
{
    //food
    Edible,
    Poisonous,
    Nutritious,

    //taste
    Delicious, //large positive
    Savoury, //small positive
    Tasteless, //small negative
    Disgusting, //large negative
    //Sweet,
    //Spicy,
    //Savory,
    //Bitter,
    //Gamey,

    //smell
    Stinky,
    Odorless,
    Aromatic,

    //secondary uses
    Fertilizer,
    Dye,

    //descriptions
    Spiky,
    Furry,
    Leathery,
    Slimy,
    Feathery,
    Scaly,
    Exoskeleton,
    Horns,
    Tail,
    Wings,

    //movement
    Stationary, //does not change position
    Bipedal,
    Quadrapedal,
    Arthropod, //insects
    Myriapod, //many feat
    Pseudopod, //no feat e.g. slimes, amoeba
    ContinuousTrack, //e.g. tank
    Rectilinear, //e.g. train
    Aerial, //anything that flies
    Aquatic, //anything that swims

    //size
    Gigantic,
    Large,
    Medium, //human size
    Small,
    Tiny,

    //emotions
    Placid, //ignores
    Friendly, //actively approaches
    Aggressive, //actively attacks
    Cowardly, //runs away
    Hungry, //will attack food stores regardless of disposition

    //actions on death
    Evaporates, /*results in*/ Gas,
    Liquifies, /*results in*/ Liquid,
    Explodes,
    Petrifies, /*results in*/ Petrified,
    Splits, //produces multiple but reduces in size
}
