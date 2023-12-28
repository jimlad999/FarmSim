namespace FarmSim.Entities;

enum Tags
{
    //special placeholder for chance resulting in no tags
    None,

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

    //category
    Meat,
    Plant,
    Wood,
    Drink,
    Rock,
    Ore,
    Gem,
    Fuel,

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

    //color
    White,
    Black,
    Red,
    Green,
    Blue,
    Yellow,

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

    //special
    Cannibal //give to mobs who eat anything from their own kind (e.g. dragon eats a dragon meat burger). results in more chance of mob fighting its own kind when food is low
}
