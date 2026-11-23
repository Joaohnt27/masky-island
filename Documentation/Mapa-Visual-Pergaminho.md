# Mapa — visual de pergaminho

O painel utiliza tons de papel envelhecido, bordas marrons e botões verde-petróleo. A ilustração existente da ilha permanece como centro da interface. O título usa Kablammo e os textos usam DynaPuff, fontes já presentes no projeto.

## Arquivos
- Modificados: cinco cenas de exploração e `Assets/Scripts/IslandMapUI.cs` (contraste dos destaques e texto de viagem).
- Criados: `Tests/UIAuthoring/AuthorMapStyle.cs` e `Tests/GameplayValidation/MapStyleValidation.cs`.

Não requer configuração manual no Inspector, novas fontes ou assets externos. A entrega do mapa pelo NPC e a restrição de viagem a locais visitados permanecem no sistema existente.

## Verificação
77 verificações aprovadas no Unity 6: fontes, textos sem transbordamento nos cinco destinos, botões, bloqueio de destino não visitado e viagem para a casa. Capturas revisadas visualmente em 1280×720. Compilação sem erros. O índice de busca do Editor registrou sua exceção conhecida, identificada separadamente dos testes de gameplay. Resultados e capturas em Documentation/MapStyle.

