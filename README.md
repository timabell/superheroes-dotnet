# Build Circle Superheroes tech test

Superheroes and supervillains are always battling it out, but how do we know who wins? This repo contains an API that gives us that answer. The API contains a `/battle` endpoint which takes a hero and a villain and returns the character that wins. 

The characters and their stats are stored in a json file stored in AWS S3 - https://s3.eu-west-2.amazonaws.com/build-circle/characters.json

Our `BattleController` pulls the json file from S3 and works out the winner by comparing the hero and villain scores from the json.

## Battle endpoint

Have a look at the `/battle` endpoint. How would you make this better?

How would you improve the tests in `./Superheroes.Tests/BattleTests.cs`?


## Weaknesses

Some superheroes have villains that they are particularly weak against. If a hero has a villain specifed in their `weakness` field then they have a point knocked off their score when fighting that villain. This can affect the outcome of the battle.

Change the `/battle` endpoint to support this functionality.


## Acceptance tests

Run the application.

1. Should return Joker - http://localhost:5000/battle?hero=Batman&villain=Joker
2. Should return Superman - http://localhost:5000/battle?hero=Superman&villain=Lex%20Luthor


## Heroes endpoint

Build an endpoint that gets the characters from the same backend and returns only the heroes, in the order of most to least awesome.