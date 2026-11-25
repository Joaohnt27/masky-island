> Atualização de animação: [equipar, guardar e minerar](../LanternAnimation/README.md). E agora equipa/acende ou guarda/apaga; a mineração guarda automaticamente.

# Lampião e perseguição da Sombra

## Fluxo
- O NPC explica o perigo, os locais onde a Sombra aparece e o uso de E.
- Resolver o primeiro enigma da casa revela a pista: o lampião fica no chão junto à lareira.
- O jogador precisa da mochila e deve clicar no lampião; o personagem se aproxima e comenta a descoberta. Ao terminar a fala, recebe uma unidade apagada.
- E acende ou apaga o lampião carregado. A luz quente acompanha o personagem e repele a Sombra a até 3 unidades. Não há consumo de combustível nesta versão.
- A perseguição começa após a primeira lembrança na praia, caverna e exterior do farol, inclusive perto do ponto de chegada. Casa e centro continuam seguros.
- A captura devolve o personagem à entrada sem remover itens ou progresso. Menus, diálogos, mineração e pausa suspendem a perseguição.
- O item e seu estado aceso/apagado persistem nas trocas de cenário durante a sessão; Novo Jogo limpa ambos. Não foi criado um sistema de save em disco.

## Arquivos criados
- `Assets/Scripts/LanternPickup.cs`: coleta por clique, pista e fala do personagem.
- `Assets/Scripts/PlayerLantern.cs`: entrada E, sprite carregado e brilho suave procedural.
- `Assets/Art/Items/Lantern/Lantern-v1.png`: arte transparente do lampião apagado.
- `Assets/Editor/AuthorLantern.cs` e `Tests/UIAuthoring/AuthorLantern.cs`: configuração reproduzível das cenas.

## Arquivos alterados
- `GameManager.cs`: item, coleta, estado da luz, objetivo no mapa e reset.
- `Interactable.cs`: E reservado ao lampião; interações continuam por clique.
- `StormPursuit.cs`: remove proteção do círculo, habilita perseguição após a primeira lembrança, integra repulsão pelo lampião.
- `DialogueController.cs`, `InventoryItemInfo.cs`, `QuestJournalUI.cs`: explicação, descrição do item e tarefa.
- Cenas Beach, House, Island, Cave, LighthouseExterior e Lighthouse: referências, ícone na mochila e apresentação do lampião; coleta e pista na casa; retirada dos círculos onde existiam.
- `AuthorStormShadow.cs`: deixa de criar o antigo círculo.
- `Tests/GameplayValidation/StormPursuitValidation.cs`: testes do fluxo novo e regressão do QTE/vídeo final.
- `README.md`: controles e mecânica atualizados.

## Configuração manual e assets faltantes
Nenhuma referência precisa ser arrastada no Inspector nas cenas entregues. Nenhum asset obrigatório está faltando. O alcance e os tempos da Sombra são configuráveis no componente StormPursuit. Recomenda-se jogar uma sessão para avaliar a dificuldade e a frequência dos encontros.

## Arte
PNG gerado para este projeto com fundo transparente. Prompt utilizado:

> Single isolated PNG game inventory prop on genuine transparent background: an UNLIT old coastal keeper's lantern, warm tarnished brass frame, brown leather handle on top, glass panes with dark visible wick inside, absolutely no flame no light glow. Hand-painted storybook adventure style matching Masky Island: expressive dark ink outlines, watercolor/gouache brush texture, ochre brass and muted teal weathering, slightly whimsical but practical portable lantern. Three-quarter front view, full object visible centered with transparent margins. No scenery, no floor, no text, no labels, no duplicate versions. Strong readable silhouette for a tiny inventory icon and carried sprite in a 2.5D game.

## Validação
49 verificações aprovadas; pista e mochila revisadas visualmente. Resultados automatizados em `results.txt`, executados no Unity 6000.6.0f1 na cópia isolada `Builds/PV`. Capturas nesta pasta documentam a apresentação revisada. O erro conhecido do índice de busca do Editor é registrado separadamente como EDITOR NOTE; não é uma exceção de gameplay.

A publicação, o GitHub e o executável Windows não foram atualizados nesta etapa.
