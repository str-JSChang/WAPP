// Muzik 67 - Interactive Intro Animation, 4-Phase Video Navigation & Minecraft Audio System

document.addEventListener("DOMContentLoaded", function () {
    initIntroAnimation();
    initScrollReveal();
    initBgmController();
    initFourPhaseVideoController();
    initMinecraftSplash();
});

// =========================================================================
// 1. STORYBOARD INTRO ANIMATION & AUDIO CHIME
// Slide 1: Full black screen with Logo in center
// Slide 2: Logo fades out
// Slide 3: Screen fades out, shifting into landing page
// =========================================================================
function initIntroAnimation() {
    const overlay = document.getElementById("introOverlay");
    const introContent = document.querySelector(".intro-content");
    const landingWrapper = document.getElementById("landingWrapper");
    const skipBtn = document.getElementById("skipIntroBtn");
    const replayBtns = document.querySelectorAll(".replay-intro-trigger");
    const soundToggleBtn = document.getElementById("soundToggleBtn");
    const audioIntro = document.getElementById("audioIntro");
    const tapPrompt = document.getElementById("introTapPrompt");

    if (!overlay) return;

    let soundEnabled = true;
    let audioCtx = null;
    let isIntroPlaying = true;
    let introAudioPlayed = false;
    let timer1 = null;
    let timer2 = null;
    let timer3 = null;

    // Harmonic chime arpeggio fallback using Web Audio API
    function playIntroChordSynth() {
        if (!soundEnabled) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            if (!audioCtx) audioCtx = new AudioContext();
            if (audioCtx.state === 'suspended') {
                audioCtx.resume();
            }

            const notes = [523.25, 659.25, 783.99, 1046.50];
            const now = audioCtx.currentTime;

            notes.forEach((freq, index) => {
                const osc = audioCtx.createOscillator();
                const gain = audioCtx.createGain();

                osc.type = 'triangle';
                osc.frequency.setValueAtTime(freq, now + index * 0.22);

                gain.gain.setValueAtTime(0.001, now + index * 0.22);
                gain.gain.exponentialRampToValueAtTime(0.18, now + index * 0.22 + 0.05);
                gain.gain.exponentialRampToValueAtTime(0.0001, now + index * 0.22 + 1.2);

                osc.connect(gain);
                gain.connect(audioCtx.destination);

                osc.start(now + index * 0.22);
                osc.stop(now + index * 0.22 + 1.3);
            });
        } catch (e) {
            console.log("Audio notice:", e);
        }
    }

    // Play chime via audio element or synth
    function playChimeSound() {
        if (!soundEnabled || introAudioPlayed) return;

        let started = false;
        if (audioIntro) {
            audioIntro.volume = 0.85;
            audioIntro.currentTime = 0;
            const promise = audioIntro.play();
            if (promise !== undefined) {
                promise.then(() => {
                    introAudioPlayed = true;
                    if (tapPrompt) tapPrompt.classList.add("fade-out");
                }).catch(() => {
                    // Autoplay blocked by browser policy; prompt user gesture
                    if (tapPrompt && isIntroPlaying) {
                        tapPrompt.classList.add("visible");
                    }
                });
                started = true;
            }
        }

        if (!started) {
            playIntroChordSynth();
            introAudioPlayed = true;
            if (tapPrompt) tapPrompt.classList.add("fade-out");
        }
    }

    // User gesture unlock for audio (pointerdown, click, or keypress anywhere)
    function onUserGesture() {
        if (!introAudioPlayed && isIntroPlaying) {
            playChimeSound();
        }
        if (tapPrompt) tapPrompt.classList.add("fade-out");
        document.removeEventListener("pointerdown", onUserGesture);
        document.removeEventListener("keydown", onUserGesture);
    }
    document.addEventListener("pointerdown", onUserGesture);
    document.addEventListener("keydown", onUserGesture);

    // Storyboard Animation Sequence
    function startIntroSequence() {
        isIntroPlaying = true;
        introAudioPlayed = false;
        clearTimeout(timer1);
        clearTimeout(timer2);
        clearTimeout(timer3);

        // Reset overlay states
        overlay.style.display = "flex";
        overlay.classList.remove("fade-out");
        if (introContent) {
            introContent.classList.remove("logo-fade-out");
        }
        if (landingWrapper) {
            landingWrapper.classList.remove("animate-landing-in");
        }

        // Try playing harmonic chime
        playChimeSound();

        // Slide 1 -> Slide 2: Logo fades out after ~2.4 seconds
        timer1 = setTimeout(function () {
            if (!isIntroPlaying) return;
            if (introContent) {
                introContent.classList.add("logo-fade-out");
            }
        }, 2400);

        // Slide 2 -> Slide 3: Screen fades out, animation shifts into landing page
        timer2 = setTimeout(function () {
            if (!isIntroPlaying) return;
            overlay.classList.add("fade-out");
            if (landingWrapper) {
                landingWrapper.classList.add("animate-landing-in");
            }
        }, 3400);

        // End of intro sequence
        timer3 = setTimeout(function () {
            overlay.style.display = "none";
            isIntroPlaying = false;
        }, 4600);
    }

    function skipIntro() {
        if (!isIntroPlaying && overlay.style.display === "none") return;
        isIntroPlaying = false;
        clearTimeout(timer1);
        clearTimeout(timer2);
        clearTimeout(timer3);

        overlay.classList.add("fade-out");
        if (landingWrapper) {
            landingWrapper.classList.add("animate-landing-in");
        }
        if (audioIntro) {
            audioIntro.pause();
        }

        setTimeout(function () {
            overlay.style.display = "none";
        }, 600);
    }

    // Toggle Intro Sound Button
    if (soundToggleBtn) {
        soundToggleBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            soundEnabled = !soundEnabled;
            soundToggleBtn.textContent = soundEnabled ? "🔊 Sound: On" : "🔇 Sound: Off";
            if (soundEnabled && !introAudioPlayed) {
                playChimeSound();
            } else if (!soundEnabled && audioIntro) {
                audioIntro.pause();
            }
        });
    }

    if (skipBtn) {
        skipBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            skipIntro();
        });
    }

    if (tapPrompt) {
        tapPrompt.addEventListener("click", function (e) {
            e.stopPropagation();
            playChimeSound();
        });
    }

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && isIntroPlaying) {
            skipIntro();
        }
    });

    replayBtns.forEach(function (btn) {
        btn.addEventListener("click", function () {
            window.scrollTo({ top: 0, behavior: "smooth" });
            startIntroSequence();
        });
    });

    // Run storyboard sequence on page load
    startIntroSequence();
}

// =========================================================================
// 2. SCROLL REVEAL OBSERVER
// =========================================================================
function initScrollReveal() {
    const revealElements = document.querySelectorAll(".scroll-reveal");
    if (!revealElements.length) return;

    if ("IntersectionObserver" in window) {
        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add("revealed");
                        observer.unobserve(entry.target);
                    }
                });
            },
            {
                threshold: 0.15,
                rootMargin: "0px 0px -40px 0px",
            }
        );

        revealElements.forEach((el) => observer.observe(el));
    } else {
        revealElements.forEach((el) => el.classList.add("revealed"));
    }
}

// =========================================================================
// 3. MINECRAFT AMBIENT WATER AUDIO CONTROLLER
// Seamless crossfade between surfacewater.m4a and underwater.m4a loops
// =========================================================================
let isAudioEnabled = true;
window.currentWaterState = 'SURFACE';

const audioSurfaceWater = document.getElementById("audioSurfaceWater");
const audioUnderwater = document.getElementById("audioUnderwater");

const TARGET_AMBIENT_VOL = 0.55;
if (audioSurfaceWater) audioSurfaceWater.volume = TARGET_AMBIENT_VOL;
if (audioUnderwater) audioUnderwater.volume = TARGET_AMBIENT_VOL;

// Smooth crossfade between surface and underwater ambience
function crossfadeAmbient(toState) {
    if (!isAudioEnabled) {
        if (audioSurfaceWater) audioSurfaceWater.pause();
        if (audioUnderwater) audioUnderwater.pause();
        return;
    }

    const currentToPlay = (toState === 'UNDERWATER') ? audioUnderwater : audioSurfaceWater;
    const currentToStop = (toState === 'UNDERWATER') ? audioSurfaceWater : audioUnderwater;

    if (currentToPlay) {
        currentToPlay.volume = Math.min(currentToPlay.volume, 0.05);
        if (currentToPlay.paused) {
            currentToPlay.play().catch(() => {});
        }
    }

    const steps = 10;
    const intervalTime = 50; // 500ms total
    let step = 0;

    const fadeInterval = setInterval(() => {
        step++;
        const progress = step / steps;

        if (currentToPlay) {
            currentToPlay.volume = Math.min(TARGET_AMBIENT_VOL, progress * TARGET_AMBIENT_VOL);
        }
        if (currentToStop) {
            currentToStop.volume = Math.max(0, (1 - progress) * TARGET_AMBIENT_VOL);
        }

        if (step >= steps) {
            clearInterval(fadeInterval);
            if (currentToStop) {
                currentToStop.pause();
                currentToStop.volume = TARGET_AMBIENT_VOL;
            }
            if (currentToPlay) {
                currentToPlay.volume = TARGET_AMBIENT_VOL;
            }
        }
    }, intervalTime);
}

function updateAmbientAudio(state) {
    if (!isAudioEnabled) {
        if (audioSurfaceWater) audioSurfaceWater.pause();
        if (audioUnderwater) audioUnderwater.pause();
        return;
    }

    if (state === 'SURFACE') {
        if (audioUnderwater) audioUnderwater.pause();
        if (audioSurfaceWater && audioSurfaceWater.paused) {
            audioSurfaceWater.play().catch(() => {});
        }
    } else if (state === 'UNDERWATER') {
        if (audioSurfaceWater) audioSurfaceWater.pause();
        if (audioUnderwater && audioUnderwater.paused) {
            audioUnderwater.play().catch(() => {});
        }
    }
}

// =========================================================================
// 4. FOUR-PHASE MINECRAFT VIDEO CONTROLLER
//    Phase 1: Above water surface loop (0.5s - 9.2s)
//    Phase 2: Diving into water transition (9.4s - 11.5s)
//    Phase 3: Underwater journey loop (11.8s - 24.5s)
//    Phase 4: Resurface transition (24.8s - 28.0s)
// =========================================================================
function initFourPhaseVideoController() {
    const video = document.getElementById("landingBgVideo");
    const exploreBtn = document.getElementById("exploreUnderwaterBtn");
    const resurfaceBtn = document.getElementById("resurfaceBtn");
    if (!video) return;

    // Video Timeline Markers
    const SURFACE_START = 0.5;
    const SURFACE_END = 9.2;
    const DIVE_START = 9.4;
    const DIVE_END = 11.5;
    const UNDERWATER_START = 11.8;
    const UNDERWATER_END = 24.5;
    const RESURFACE_START = 24.8;
    const RESURFACE_END = 28.0;

    // 'SURFACE' | 'DIVING' | 'UNDERWATER' | 'RESURFACING'
    let currentState = 'SURFACE';
    window.currentWaterState = currentState;
    let isTransitioning = false;

    // Keep video playing muted in background
    video.muted = true;
    video.play().catch(() => {});

    // Active playback loop controller
    video.addEventListener("timeupdate", function () {
        const time = video.currentTime;

        if (currentState === 'SURFACE') {
            if (time >= SURFACE_END || time < SURFACE_START - 0.2) {
                video.currentTime = SURFACE_START;
            }
        } else if (currentState === 'DIVING') {
            if (time >= DIVE_END) {
                currentState = 'UNDERWATER';
                window.currentWaterState = currentState;
                isTransitioning = false;
                video.currentTime = UNDERWATER_START;
            }
        } else if (currentState === 'UNDERWATER') {
            if (time >= UNDERWATER_END || time < UNDERWATER_START - 0.2) {
                video.currentTime = UNDERWATER_START;
            }
        } else if (currentState === 'RESURFACING') {
            if (time >= RESURFACE_END) {
                currentState = 'SURFACE';
                window.currentWaterState = currentState;
                isTransitioning = false;
                video.currentTime = SURFACE_START;
            }
        }
    });

    // Dive Transition Action
    function diveUnderwater(targetScroll = true) {
        if (currentState === 'UNDERWATER' || currentState === 'DIVING') return;
        currentState = 'DIVING';
        window.currentWaterState = currentState;
        isTransitioning = true;

        // Smoothly crossfade from surface ambience to underwater ambience
        crossfadeAmbient('UNDERWATER');

        video.currentTime = DIVE_START;
        if (video.paused) video.play().catch(() => {});

        if (targetScroll) {
            const realms = document.getElementById("realmNavigation");
            if (realms) {
                realms.scrollIntoView({ behavior: "smooth" });
            }
        }
    }

    // Resurface Transition Action
    function resurfaceToOverworld(targetScroll = true) {
        if (currentState === 'SURFACE' || currentState === 'RESURFACING') return;
        currentState = 'RESURFACING';
        window.currentWaterState = currentState;
        isTransitioning = true;

        // Smoothly crossfade from underwater ambience back to surface ambience
        crossfadeAmbient('SURFACE');

        video.currentTime = RESURFACE_START;
        if (video.paused) video.play().catch(() => {});

        if (targetScroll) {
            window.scrollTo({ top: 0, behavior: "smooth" });
        }
    }

    // Wire Explore Button
    if (exploreBtn) {
        exploreBtn.addEventListener("click", function (e) {
            e.preventDefault();
            diveUnderwater(true);
        });
    }

    // Wire Resurface Button
    if (resurfaceBtn) {
        resurfaceBtn.addEventListener("click", function (e) {
            e.preventDefault();
            resurfaceToOverworld(true);
        });
    }

    // Wire Realm Nav Links
    const realmLinks = document.querySelectorAll('a[href="#realmNavigation"]');
    realmLinks.forEach(function (link) {
        link.addEventListener("click", function () {
            diveUnderwater(false);
        });
    });

    // Wire Overworld/Home Links
    const homeLinks = document.querySelectorAll('a[href="/Index"], a[href="/"], .nav-brand-text, .brand-wrapper');
    homeLinks.forEach(function (link) {
        link.addEventListener("click", function (e) {
            if (window.location.pathname === '/' || window.location.pathname.endsWith('/Index')) {
                if (currentState === 'UNDERWATER' || currentState === 'DIVING') {
                    e.preventDefault();
                    resurfaceToOverworld(true);
                }
            }
        });
    });

    // Subtle scroll observer
    let lastScrollY = window.scrollY;
    let scrollTicking = false;

    window.addEventListener("scroll", function () {
        if (!scrollTicking) {
            window.requestAnimationFrame(function () {
                const currentScrollY = window.scrollY;
                const realms = document.getElementById("realmNavigation");
                const realmTop = realms ? realms.offsetTop - 300 : 700;

                if (!isTransitioning) {
                    if (currentScrollY > realmTop && currentState === 'SURFACE') {
                        diveUnderwater(false);
                    } else if (currentScrollY < 150 && currentState === 'UNDERWATER') {
                        resurfaceToOverworld(false);
                    }
                }

                lastScrollY = currentScrollY;
                scrollTicking = false;
            });
            scrollTicking = true;
        }
    }, { passive: true });
}

// =========================================================================
// 5. AMBIENCE TOGGLE CONTROLLER
// =========================================================================
function initBgmController() {
    const toggleBtn = document.getElementById("bgmToggleBtn");
    const icon = document.getElementById("bgmIcon");
    const label = document.getElementById("bgmLabel");

    if (!toggleBtn) return;

    toggleBtn.addEventListener("click", function () {
        isAudioEnabled = !isAudioEnabled;

        if (isAudioEnabled) {
            toggleBtn.classList.add("playing");
            if (icon) icon.className = "bi bi-volume-up-fill text-success";
            if (label) label.textContent = "Audio: On";
            updateAmbientAudio(window.currentWaterState || 'SURFACE');
        } else {
            toggleBtn.classList.remove("playing");
            if (icon) icon.className = "bi bi-volume-mute-fill";
            if (label) label.textContent = "Audio: Off";
            if (audioSurfaceWater) audioSurfaceWater.pause();
            if (audioUnderwater) audioUnderwater.pause();
        }
    });

    // First user gesture triggers initial ambient audio
    const enableAudioOnFirstClick = () => {
        if (isAudioEnabled) {
            updateAmbientAudio(window.currentWaterState || 'SURFACE');
        }
        document.removeEventListener("click", enableAudioOnFirstClick);
    };
    document.addEventListener("click", enableAudioOnFirstClick);
}

// =========================================================================
// 6. INTERACTIVE PIANO / NOTE SYNTHESIZER (Web Audio API)
// =========================================================================
let pianoAudioCtx = null;

window.playTone = function (freq, btnElement) {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;

        if (!pianoAudioCtx) {
            pianoAudioCtx = new AudioContext();
        }
        if (pianoAudioCtx.state === 'suspended') {
            pianoAudioCtx.resume();
        }

        const now = pianoAudioCtx.currentTime;

        const osc1 = pianoAudioCtx.createOscillator();
        const osc2 = pianoAudioCtx.createOscillator();
        const gainNode = pianoAudioCtx.createGain();

        osc1.type = 'triangle';
        osc1.frequency.setValueAtTime(freq, now);

        osc2.type = 'sine';
        osc2.frequency.setValueAtTime(freq * 2, now);

        gainNode.gain.setValueAtTime(0.001, now);
        gainNode.gain.linearRampToValueAtTime(0.25, now + 0.02);
        gainNode.gain.exponentialRampToValueAtTime(0.0001, now + 1.2);

        osc1.connect(gainNode);
        osc2.connect(gainNode);
        gainNode.connect(pianoAudioCtx.destination);

        osc1.start(now);
        osc2.start(now);
        osc1.stop(now + 1.25);
        osc2.stop(now + 1.25);

        if (btnElement) {
            btnElement.classList.add("key-active");
            setTimeout(() => {
                btnElement.classList.remove("key-active");
            }, 180);
        }
    } catch (err) {
        console.error("Synthesizer error:", err);
    }
};

// =========================================================================
// 7. MINECRAFT SPLASH TEXT CONTROLLER
// Quotes matching authentic Minecraft title screen splash mechanics
// =========================================================================
function initMinecraftSplash() {
    const splashEl = document.getElementById("mcSplashText");
    if (!splashEl) return;

    const quotes = [
        "welcome to moozik",
        "muzik 67!",
        "67 67 67",
        "Son.",
        "如果我是DJ 你会爱我吗",
        "tung tung tung sahur"
    ];

    let currentIndex = 0;
    splashEl.textContent = quotes[0];

    // Clicking splash text cycles to next quote with a bounce
    splashEl.addEventListener("click", function () {
        currentIndex = (currentIndex + 1) % quotes.length;
        splashEl.textContent = quotes[currentIndex];

        // Trigger a fresh pulse
        splashEl.style.animation = "none";
        void splashEl.offsetWidth; // Force reflow
        splashEl.style.animation = "mcSplashPulse 1.8s infinite ease-in-out";
    });
}
