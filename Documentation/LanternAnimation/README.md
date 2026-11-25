# Animação do lampião e integração com mineração

O gesto usa o sprite atual do lampião, o idle direcional já existente e movimento procedural de posição, escala, rotação e transparência. Não foram substituídos os sprites do personagem nem o Animator. Equipar e guardar duram 0,45 segundo; equipado, o lampião balança suavemente e acompanha a orientação do personagem.

## Comportamento
- A coleta deixa o lampião apagado e guardado.
- E equipa e acende; E novamente apaga e guarda.
- Os controles de movimento ficam bloqueados apenas durante o gesto. Comandos repetidos não reiniciam a animação.
- Ao iniciar uma tentativa, a mineração reserva a interação, apaga a luz e aguarda o lampião terminar de ser guardado.
- Só então começam a animação corporal com a picareta, os impactos sonoros e o progresso da extração.
- Durante a mineração, E não equipa o lampião. Ao concluir ou cancelar, o lampião permanece guardado. E permite equipá-lo novamente.
- Pausa congela animações e progresso. A posse do item continua no GameManager.

## Arquivos modificados
- `Assets/Scripts/PlayerLantern.cs`: transições, idle, orientação, tecla E e espera para guardar.
- `Assets/Scripts/MiningSpot.cs`: estado de preparação antes de iniciar golpes.
- `Assets/Scripts/PlayerSpriteAnimation.cs`: aguarda a preparação antes de tocar a mineração.
- `Assets/Scripts/MiningWorldFeedback.cs` e `GameplayAudioFeedback.cs`: evitam picareta e impactos durante o gesto.
- `Assets/Scripts/MiningUI.cs`: indicação de que o lampião está sendo guardado.
- `Assets/Scripts/InventoryItemInfo.cs`: descrição dos controles atualizada.
- `Tests/GameplayValidation/StormPursuitValidation.cs`: aguarda a animação após comandos de E.

## Arquivos criados
- `Tests/GameplayValidation/LanternMiningValidation.cs`: valida equipamento, armazenamento, pausa, bloqueios e mineração das três rochas.
- Este relatório e capturas de validação.

## Configuração manual / assets faltantes
Nenhuma. As referências já configuradas nas seis cenas são reutilizadas. `Equip Duration` no PlayerLantern controla a duração do gesto.

## Testes
54 verificações aprovadas em `results.txt`. A regressão de perseguição e finalização também passou em `../Music/results.txt`. A espera do teste foi ajustada para aguardar o término da recuperação da Sombra após a animação de guardar, antes de iniciar o próximo encontro. Execução no Unity 6000.6.0f1, usando a cópia isolada Builds/PV. O executável Windows e a publicação não foram alterados nesta etapa.
