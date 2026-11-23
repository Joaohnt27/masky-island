# Mineração no cenário e inventário de cristais

## Ideia e fluxo

O desafio continua tendo duas partes: **encontrar os cristais minerando** e **interpretar as pistas dos encaixes**. A coleta agora acontece no cenário.

1. Conclua a conversa com o estranho e resolva o pergaminho.
2. Clique na rocha na parte inferior direita da ilha. O personagem caminha até o ponto de mineração.
3. Ele para e movimenta uma picareta durante a extração de três segundos. O painel pequeno mostra o progresso e permite cancelar.
4. Ao terminar, um cristal aparece no inventário no canto superior esquerdo. Clique novamente na rocha para procurar o próximo.
5. Após três extrações completas, os três cristais estarão disponíveis e o painel de encaixes será liberado.
6. Selecione um cristal e o encaixe cuja pista combina com sua forma. O item passa a aparecer como ENCAIXADO no inventário. Erros não consomem itens.
7. Complete os três encaixes para recuperar a lembrança da tempestade.

Existe **uma rocha minerável com três cristais**, em sequência garantida (triângulo, círculo e quadrado). O jogador não escolhe a recompensa antes de minerar. Não há moedas, falha aleatória ou timer de missão. A rocha deixa de oferecer extrações depois dos três cristais. Fechar os encaixes permite continuar explorando; sair e voltar à área reabre o painel.

O inventário é específico dos três cristais deste puzzle: não é um sistema genérico de equipamentos. Os itens persistem entre Island e Cave durante a sessão. Reiniciar ou voltar ao menu limpa o estado; não há salvamento em disco.

## Arquivos criados

- `Assets/Scripts/CrystalInventoryUI.cs` e `.meta`: inventário visual atualizado pelos eventos do GameManager existente.
- `Assets/Scripts/MiningWorldFeedback.cs` e `.meta`: movimento da picareta e pequeno impacto na rocha durante a extração.
- `Assets/Art/Materials/MiningBoulder.asset`, materiais `MiningRock`, `MiningRockLight`, `MiningRockFacet`, `MiningHandle`, `MiningMetal` e respectivos `.meta`: geometria e cores simples da rocha/ferramenta.
- `Tests/UIAuthoring/AuthorWorldMining.cs`: autoria das referências e objetos nas cenas em cópia isolada; não executar novamente sobre as cenas prontas.
- `Tests/GameplayValidation/WorldMiningValidation.cs` e `RunWorldMining.ps1`: validação reproduzível do novo fluxo. Esta suíte substitui as expectativas antigas de coleta pelo painel da CrystalPuzzleValidation.

## Arquivos modificados

- `Assets/Scripts/MiningSpot.cs`: ponto de aproximação e descoberta do próximo cristal ainda não coletado. Reutiliza a coroutine, os bloqueios e o inventário existente no GameManager.
- `Assets/Scripts/PlayerClickMovement.cs`: clique na rocha, aproximação e início da extração ao chegar. Clique no chão ou na porta cancela a intenção anterior; preservado o movimento existente.
- `Assets/Scripts/MiningUI.cs`: coleta com aviso compacto no cenário; seleção de formas fica disponível somente na etapa de encaixes.
- `Assets/Scripts/DialogueController.cs`: orientação do NPC explica clique na rocha e entrega no inventário.
- `Assets/Scenes/Island.unity`: rocha com collider, ponto de aproximação, ferramenta, feedback e inventário configurados.
- `Assets/Scenes/Cave.unity`: inventário visual local ligado ao mesmo estado persistente.

## Configuração manual no Unity

Nenhuma ligação adicional necessária. As cenas salvas já possuem referências, materiais, collider, ponto de aproximação, textos e botão de cancelamento. MiningSpot continua usando detecção por distância, sem Rigidbody; seu collider serve ao clique por raycast. O GameManager continua sendo a única fonte de dados, sem novas referências persistentes para a UI.

## Assets e limitações

A rocha e a picareta usam formas simples criadas no projeto. O movimento da ferramenta é um emote por código; não é uma animação completa do corpo do personagem. Ainda faltam sprites/clips de mineração para esse refinamento. Nenhum pacote externo foi adicionado.

## Testes

- 36 verificações de mineração/inventário aprovadas em Play Mode: clique sintético pelo New Input System, raycast da rocha, aproximação, picareta, bloqueio de movimento, pausa, cancelamento, entrega dos três cristais, ausência de duplicatas, persistência entre cenas, encaixes e reinício.
- 34 verificações de regressão do primeiro puzzle aprovadas. Total: 70 verificações.
- Compilação sem erros de código. A exceção conhecida do índice de busca do Editor foi registrada separadamente; não houve exceções de gameplay nas suítes aprovadas.
- Capturas do cenário e HUD renderizadas pela Unity em 1280 × 720 e inspecionadas. A rocha e o personagem ficam visíveis durante a extração. Outras proporções de tela e uma sessão manual longa de navegação ainda não foram verificadas.
- O teste de clique usa mouse virtual e direcionamento de entrada para o jogo somente na cópia de validação, pois o Editor batch está sem foco. As configurações de entrada do projeto principal não foram alteradas.
- Relatórios: WorldMiningValidationResults.txt e FirstPuzzleRegressionAfterWorldMining.txt. Capturas: world-mining-action.png, world-mining-inventory.png e world-mining-memory.png.

