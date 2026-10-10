// Gerado por online/construir.ts a partir de tcg/src/tipos.ts. Não edite: edite o .ts e reconstrua.
// Tipos do motor do Arcanoclisma TCG. Só sintaxe "apagável" (sem enum), para rodar direto no Node 24.

                            
                                                                    
/** 'item': equipamento do Conjurador (Canal, Armadura ou Extra), escolhido na preparação, fora do baralho. */
                                                                                   
                                                           
                                                  
                                                                                                     

// ---------- construtor de efeitos (pedido do autor, 04/10) ----------
/** Quando uma habilidade acontece. */
                                                                                                                                            
                                                                                                                                                      
                                                          
                                                                                                                                              
                        
     
                                                                                                                      
                                                                                                                        
                                                                                                                     
                                                                                                                         
                                                                                                                
     
                                                                                                                                           
                                                                                                                                     
              
/** De quem é o evento que dispara a habilidade (eventos de jogador, 05/10). Sem o campo: qualquer jogador. */
                                                          
/** Em qual turno: do dono da carta, do oponente ou em cada turno. */
                                                  
/** Custo de uma habilidade ativada ("virar/pagar"). */
                                                                                                                           
/**
 * Custo pago numa decisão (05/10, decisão 40): Arcano (reserva, Grau levantado ou cartas da mão enviadas ao Arcano), Vida
 * (o Conjurador perde), descartar cartas da mão, mandar cartas do Grau ao cemitério e sacrificar Aliados (o pagador escolhe).
 */
                                                                                                                                 
/**
 * Custo condicional (05/10, decisão 40), em três lugares: no efeito (`Efeito.custo`, "pague para funcionar"), no gatilho
 * (`Habilidade.pagar`) e na manutenção (`Habilidade.manutencao`). quem: 'voce' (padrão) paga para o efeito acontecer;
 * 'oponente' paga para IMPEDIR ("a menos que o oponente pague X"). Pagar é sempre uma escolha (pendência pagarCusto).
 * quando: 'resolver' (padrão) ou 'jogar' (decidido ao jogar a carta; só em efeitos ao jogar/ao entrar).
 * senao = o que acontece quando o efeito é impedido (você não pagou / o oponente pagou): 'linha' (só esta linha não
 * acontece; padrão), 'carta' (a carta é anulada: os efeitos seguintes não acontecem e, decidido ao jogar, ela sai da
 * pilha), 'alternativo' (os `alternativos` acontecem no lugar); na manutenção também 'sai' (a carta vai para `destino`)
 * e 'pula' (o efeito da habilidade não acontece neste turno).
 */
                                                      
                             
                                
                                                             
                          
                                                        
 
/** Manutenção (05/10): custo cobrado numa etapa de um turno enquanto a carta está em campo (padrão: início do seu turno; sem pagar, sai para o cemitério). */
                                                                                         
/** Passivo "enquanto estiver em campo" (05/10): bônus e palavras-chave a um grupo, recalculados sempre (não gravados nas cartas). */
                                                                                                                                                                                                                                                                      
/** Passivo (05/10): cartas de um jogador custam `valor` a mais (negativo = a menos), dos tipos indicados (vazio = todas). */
                                                                                                      
/** Habilidade montada no Editor: condição + efeitos (que se acumulam, em ordem). */
                             
                                                             
                                                                                                
                                                                                                            
                                                                                             
                                                                                                                                                               
                                                                                                             
                                                                                                          
                                                                                                                                                            
                                                                                                                                                       
                                                                 
                                                                               
                                                                                                                   
                                                                                                                                        
                                                                                       
 
/**
 * Protegido (unificado em 04/10, noite; antes havia "Protegido N" e "Proteção mágica"): quem mira esta carta com carta ou
 * ativação do oponente paga isto; sem poder pagar, não pode mirar. `protegido: N` (número) é o mesmo que `{ arcano: N }`.
 */
                                                                                                                               
/** @deprecated Nome antigo ("Proteção mágica"); use Protegido. */
                                 
/** Para onde vão as cartas escolhidas. */
                                                                                                                  
                                                                                              
/**
 * Concessão (pedido do autor, 04/10): palavras-chave e/ou condições dadas a uma carta em campo, definidas nos dados.
 * palavras: qualquer palavra-chave que o motor entende ("Etéreo", "Protegido 2", "Reanimum"...); ataque/vida: modificador
 * permanente enquanto está em campo (pode ser negativo); turnos: fica X turnos do seu controlador em campo (conta o da
 * entrada); destino: para onde vai ao sair por turnos (padrão: cemitério).
 */
                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                            

/**
 * Requisito de uma linha de efeito, habilidade ou ação (pedido do autor, 04/10), definido no Editor:
 * grau = mínimo de cartas no seu Grau para acontecer; arcano = Arcano pago na hora (reserva, depois Grau levantado).
 * Sem cumprir, aquela linha não acontece (as outras da carta continuam).
 */
                                                            
     
                                                                                                                       
                                                              
     
                  

/**
 * Quem escolhe o alvo de um efeito de alvo único (pedido do autor, 04/10): quem jogou/ativou (padrão), o jogador atingido
 * ou ao acaso. Nada escolhe o alvo sozinho; "atinge tudo" é o alcance "todos".
 */
                                                                  
/**
 * Efeito secundário (pedido do autor, 04/10, noite): custo ou consequência que acompanha outro efeito, como linha extra
 * ("...; depois, seu Conjurador toma 2 de dano"). Só muda o texto e a avaliação do bot; o motor resolve em ordem.
 */
                                   
                                                                     
                                                                                                             
                                                                                                          
  
/** Quem sofre o "tirar da mão": quem jogou/ativou, o oponente ou os dois. */
                                                      
                 
                                                                                                                  
                                                                           
                                                              
                                                                                    
                                              
                                                                                                                                
                                                                                                              
                                                    
                                                                 
                                                                                                                                                   
                                                      
                                                               
                                      
                                     
                                             
                                                                                                                               
                                                   
                                                                                                                                                         
                                            
                                                                                                                                                                
     
                                                                                                                          
                                                                                                                                 
                                                                                                                           
                                                                                                        
     
                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                  
                                         
                                                                                                           
                                                            
                                                                                                                       
                                            
                                                                                                                             
                                                                                                                                                
         
                                                                                                                              
                                                                                                                                        
         
                      
                                                                             
                                                    
     
                                                                                                                         
                                                                                                                              
                                                                                                                
     
                                                                                                                                                 
                                                                                           
                                                                
                                                                                                        
                                                                                                  
                                                                                                                          
                                                         
                                                                                                       
                                                                                                                                                        
                                                                                               
                                                                                                                                                                                                                                                                         
                                                                                         
                                                                                                                                      
     
                                                                                                                           
                                                                                                                                
     
                                                                     
                                                                                                                                              
                                                                          
     
                                                                                                                        
                                                                                                                
     
                                                                     
                                                                                                              
                                          
                                                                                                                                  
                                                                                 

/** Arma: efeitos ao atacar (pedido do autor, 04/10). */
                           
                                                                               
                     
                                                                                                    
                        
                                                                             
                            
 

/** Habilidade ativada de uma permanente: paga custo em Arcano e (opcionalmente) suspende. */
                           
                                                                                                                                                           
                      
                
                    
                    
                                                                     
                               
                                                                                           
                     
                                                      
                            
                                                                                 
                    
                                                                                          
                        
 

                         
                                                                                            
                        
                                                           
                                   
                                                                                                       
                                      
                                                                                                
                            
                                                                
                                       
                                                                                                                               
                        
                                                                  
                            
                                                 
                               
                                                                          
                    
                                                                                                                             
                            
                                                                                                         
                                     
                                                                            
                                    
 

                           
             
               
                  
                     
                    
                
               
                                                                     
                          
                                                              
                        
                         
                                                                  
                       
                                                                                                                                                                                                                                                                                                                    
                           
                  
                
                                                                                                        
                     
                                                             
                      
                                                                                         
                           
                                   
                     
                                                  
                      
                                                                                       
                              
     
                                                                                                                          
                                                                                                                                        
     
                                 
                                                                                           
                                
                                                                                                                       
                                              
                                                                                                           
                                                 
                                                  
                         
                                                                        
                  
     
                                                                                                                                
                                                                                                                               
     
                      
                                                                                                                      
                    
                                                                                                                                              
                       
                                                                                                                                       
                                  
     
                                                                                                                    
                                                                                    
     
                      
                                                                           
                             
                                                                                                                                     
                       
                                                                                          
                        
                                                                      
                    
                                                                              
                  
                                                                                         
                        
                  
                
 

/** Habilidade própria do Conjurador (rodada 7): uma ativada, um gatilho ou uma passiva. */
                                  
                                                                                                                      
                      
                                                                            
                                                                                                                                                                          
                                                                                             
                                    
                                                                               
                                   
                                                                                         
                                       
                                               
                             
                                                                                              
                                    
     
                                                                                                                           
                                                                                                                              
                                                                                                                                   
     
                             
 

/**
 * Permissão contínua do Conjurador (Editor, 04/10): pode jogar do seu cemitério ou banimento cartas dos tipos indicados
 * (padrão: qualquer uma) e custo até `custoMax`, pagando o custo normal, na velocidade normal da carta. `banir`: a carta
 * jogada assim vai para o banimento ao resolver ou ao sair do campo (como o Reanimum). `umaVezPorTurno`: só uma carta por turno.
 */
                                                                                                                                                    

                                
             
               
                    
                                                                                                
               
                     
                      
                     
                      
                     
                                                                      
                          
                                                              
                    
 

                           
                                   
                                              
 

                             
                     
                   
                                                                                         
                   
 

                       
             
                
                        
                
                    
               
                    
                                                         
                        
                                                                                           
                           
                                                   
                            
                                                                   
                           
                                                                    
                       
                     
                            
                          
                                                         
                       
                                                                   
                                   
                                                                 
                           
                                                                                              
                                                                                  
                                                                                                                         
                           
                                                       
                                                               
                                                                                            
                             
                                                                     
                        
                                                                        
                                               
                                                                         
                                        
                                                                                       
                                     
 

/** Todo dano é dano; só muda a fonte. Efeitos podem ser anulados; combate não. */
                                             

/** Estado por turno das habilidades de Conjurador, por jogador. */
                          
                                                       
                                                          
                   
                                                               
                             
                                                                   
                      
                                                                                                    
                    
                                                                                                
                   
                                                
                      
                                                                                                      
                        
 

                               
                       
                    
                
                  
                      
                 
                       
                                                                                                                      
                      
                                                                                              
                  
                 
 

                       
                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                     
                                                                                                                                                           
                                                                                                      
                                                                                                                                                     
                                                                                         

                       
                                                
                                                               
                                          
                                                                                  
                                                
                                                                               
                                                                                 
                                                                                                                               
                                                                                                                                                                                                                                                                                                                                                                                                                                                
     
                                                                                                                            
                                                                                                                    
                                                                                                                    
     
                                                                                                                                                                                                                     

/** Efeitos que esperam uma escolha terminar para continuar (efeitos acumulados). */
                                                                                

                          
                   
               
                                                                                  
                            
                                                                                                
                         
                                                                                                                 
                                        
                                                                              
                         
                                                          
 

                         
                     
                                          
                                   
                     
                
                 
                              
                      
                 
                              
                          
                                      
                          
                 
                                
                    
                                                                   
                       
                                                                                                 
                                  
                       
                                                                                             
                                   
                                                                                                      
                              
              
                 
                
 

                     
                                                                   
                                                            
                                                              
                                                                                                                                                                                                                                                                              
                                                                                                 
                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                       
                                                             
                                                                                                                                                                                                                                                                                                                                                                                                         
                                                                                        
                                                                                                                                                                          
                                                                                     
                                                                  
                                                                                                                                                                                                     
                                         
