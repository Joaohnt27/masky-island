# Etapa 09 — Mapa ilustrado entregue pelo NPC

## Alteração
O mapa agora usa uma ilustração original em estilo pergaminho, tinta e guache: praia, casa, poço central, caverna e farol conectados por caminhos. Os nomes e estados continuam em TextMeshPro, com botões separados da imagem para manter legibilidade e interação.

O jogador começa sem mapa. Ao terminar a conversa inicial, o NPC entrega o mapa e explica M e viagem rápida. Antes da entrega, M e chamadas diretas não abrem o mapa; o botão e a indicação de M ficam ocultos. Interromper o diálogo não concede o item. Visitas anteriores continuam registradas, permitindo voltar à praia/casa após receber o mapa. O item persiste entre cenários e é removido em nova partida.

## Arquivos criados
- Assets/Art/Maps/IslandMap-v1.png e metadados: arte final usada pelas cinco cenas.
- Tests/UIAuthoring/AuthorIllustratedMap.cs: integração incremental da arte e reposicionamento dos botões.
- Documentation/IllustratedMap/GenerationPrompt.txt: prompt final e método de geração.

## Arquivos modificados
- Assets/Scripts/GameManager.cs: HasMap, entrega idempotente e limpeza em ResetMission; StartMission sozinho não concede mapa.
- Assets/Scripts/DialogueController.cs: fala de entrega e concessão no fim do diálogo inicial.
- Assets/Scripts/IslandMapUI.cs: bloqueio de acesso/viagem, botão e indicação de tecla condicionados à posse do mapa.
- Assets/Scenes/Beach.unity, House.unity, Island.unity, Cave.unity e Lighthouse.unity: ilustração, referências e botões.
- Tests/GameplayValidation/MapAudioValidation.cs: cenário de entrega pelo NPC, interrupção, reinício e validação de botões sobre a nova imagem.

## Configuração manual e assets
Nenhuma configuração manual no Inspector. Nenhum asset obrigatório faltante. Geração pela ferramenta integrada image_gen, sem API/CLI externa. A imagem está dentro do projeto e não depende da pasta de geração do Codex.

## Testes
- Compilação Unity 6 sem erros.
- 50 verificações aprovadas: IllustratedMap/ValidationResults.txt.
- Cobertura: bloqueio antes da entrega, conversa interrompida, entrega ao final, recompensa única, persistência entre cenas, nova partida, missão iniciada sem entrega, botões alcançáveis por raycast, viagem rápida, cristais/lembranças preservados, bloqueios de interação e efeitos sonoros.
- Captura revisada em 1280×720: IllustratedMap/InGameMap.png. Arte visível e destinos clicáveis legíveis.
- Nota de ambiente: a exceção conhecida de UnityEditor.Search.SearchDatabase foi registrada separadamente. Nenhuma exceção de gameplay aceita.
- Não foi repetido o percurso completo até os créditos nesta etapa; a validação foi direcionada às mudanças do mapa e à sua integração.
