// Muzik 67 - Interactive Intro Animation, 4-Phase Video Navigation & Web Audio Synthesizer

document.addEventListener("DOMContentLoaded", function () {
    initIntroAnimation();
    initScrollReveal();
    initBgmController();
    initFourPhaseVideoController();
});

// =========================================================================
// 1. STORYBOARD INTRO ANIMATION
// =========================================================================
function initIntroAnimation() {
    const overlay = document.getElementById("introOverlay");
    const introContent = document.querySelector(".intro-content");
    const landingWrapper = document.getElementById("landingWrapper");
    const skipBtn = document.getElementById("skipIntroBtn");
    const replayBtns = document.querySelectorAll(".replay-intro-trigger");
    const soundToggleBtn = document.getElementById("soundToggleBtn");

    if (!overlay) return;

    let soundEnabled = true;
    let audioCtx = null;
    let isIntroPlaying = true;
    let timer1 = null;
    let timer2 = null;
    let timer3 = null;

    // Optional audio chime synthesis using Web Audio API
    function playIntroChord() {
        if (!soundEnabled) return;
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            if (!audioCtx) {
                audioCtx = new AudioContext();
            }
            if (audioCtx.state === 'suspended') {
                audioCtx.resume();
            }

            // Play a cheerful music chime arpeggio: C5 (523.25Hz), E5 (659.25Hz), G5 (783.99Hz), C6 (1046.50Hz)
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

    // Storyboard Animation Sequence
    function startIntroSequence() {
        isIntroPlaying = true;
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
        playIntroChord();

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

        setTimeout(function () {
            overlay.style.display = "none";
        }, 600);
    }

    // Skip button click
    if (skipBtn) {
        skipBtn.addEventListener("click", skipIntro);
    }

    // Keyboard support: Escape or Space skips intro
    document.addEventListener("keydown", function (e) {
        if (isIntroPlaying && (e.key === "Escape" || e.code === "Space")) {
            e.preventDefault();
            skipIntro();
        }
    });

    // Sound toggle button
    if (soundToggleBtn) {
        soundToggleBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            soundEnabled = !soundEnabled;
            soundToggleBtn.textContent = soundEnabled ? "🔊 Sound: On" : "🔇 Sound: Off";
            if (soundEnabled) {
                playIntroChord();
            }
        });
    }

    // Replay intro triggers
    replayBtns.forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: "smooth" });
            startIntroSequence();
        });
    });

    // Start on first visit
    startIntroSequence();
}

// =========================================================================
// 2. SCROLL REVEAL (IntersectionObserver)
// =========================================================================
function initScrollReveal() {
    const revealElements = document.querySelectorAll(".scroll-reveal");
    if (!revealElements || revealElements.length === 0) return;

    if (!("IntersectionObserver" in window)) {
        revealElements.forEach(el => el.classList.add("visible"));
        return;
    }

    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add("visible");
                obs.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.15,
        rootMargin: "0px 0px -40px 0px"
    });

    revealElements.forEach(el => observer.observe(el));
}

// =========================================================================
// 3. FOUR-PHASE MINECRAFT VIDEO CONTROLLER
//    Phase 1: Above water surface loop (0.5s - 9.2s)
//    Phase 2: Diving into water transition (9.4s - 11.5s) + Dive audio cue
//    Phase 3: Underwater journey loop (11.8s - 24.5s)
//    Phase 4: Resurface transition (24.8s - 28.0s) + Surface audio cue
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
    let isTransitioning = false;

    // Keep video playing
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
                isTransitioning = false;
                video.currentTime = SURFACE_START;
            }
        }
    });

    // Dive Transition Action
    function diveUnderwater(targetScroll = true) {
        if (currentState === 'UNDERWATER' || currentState === 'DIVING') return;
        currentState = 'DIVING';
        isTransitioning = true;
        playWaterDiveSound();

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
        isTransitioning = true;
        playWaterSurfaceSound();

        video.currentTime = RESURFACE_START;
        if (video.paused) video.play().catch(() => {});

        if (targetScroll) {
            window.scrollTo({ top: 0, behavior: "smooth" });
        }
    }

    // Button Click: "Explore the Underwater"
    if (exploreBtn) {
        exploreBtn.addEventListener("click", function (e) {
            e.preventDefault();
            diveUnderwater(true);
        });
    }

    // Button Click: "Resurface to Overworld"
    if (resurfaceBtn) {
        resurfaceBtn.addEventListener("click", function (e) {
            e.preventDefault();
            resurfaceToOverworld(true);
        });
    }

    // Scroll trigger
    let scrollDebounce = null;
    window.addEventListener("scroll", function () {
        if (isTransitioning) return;
        const scrollY = window.scrollY;

        clearTimeout(scrollDebounce);
        scrollDebounce = setTimeout(function () {
            // Scroll down past hero (scrollY > 220) triggers dive into underwater
            if (scrollY > 220 && currentState === 'SURFACE') {
                diveUnderwater(false);
            }
            // Scroll back up into hero (scrollY < 120) triggers resurface
            else if (scrollY < 120 && currentState === 'UNDERWATER') {
                resurfaceToOverworld(false);
            }
        }, 80);
    }, { passive: true });
}

// =========================================================================
// 4. WATER DIVE & SURFACE AUDIO CUES (Web Audio API)
// =========================================================================
function playWaterDiveSound() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;
        const ctx = new AudioContext();
        const now = ctx.currentTime;

        // 1. Splash noise burst (bubble impact)
        const bufferSize = Math.floor(ctx.sampleRate * 0.45);
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }

        const noise = ctx.createBufferSource();
        noise.buffer = buffer;

        // Resonant low-pass sweep: head entering water
        const filter = ctx.createBiquadFilter();
        filter.type = "lowpass";
        filter.frequency.setValueAtTime(950, now);
        filter.frequency.exponentialRampToValueAtTime(140, now + 0.4);

        const gain = ctx.createGain();
        gain.gain.setValueAtTime(0.35, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.45);

        noise.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        noise.start(now);

        // 2. Underwater bubble gurgles
        [220, 165, 130].forEach((freq, idx) => {
            const osc = ctx.createOscillator();
            const oscGain = ctx.createGain();
            osc.type = "sine";
            osc.frequency.setValueAtTime(freq, now + idx * 0.09);
            osc.frequency.exponentialRampToValueAtTime(freq * 0.75, now + idx * 0.09 + 0.16);

            oscGain.gain.setValueAtTime(0.001, now + idx * 0.09);
            oscGain.gain.linearRampToValueAtTime(0.25, now + idx * 0.09 + 0.02);
            oscGain.gain.exponentialRampToValueAtTime(0.0001, now + idx * 0.09 + 0.18);

            osc.connect(oscGain);
            oscGain.connect(ctx.destination);
            osc.start(now + idx * 0.09);
            osc.stop(now + idx * 0.09 + 0.2);
        });
    } catch (e) {
        console.log("Audio notice:", e);
    }
}

function playWaterSurfaceSound() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;
        const ctx = new AudioContext();
        const now = ctx.currentTime;

        const bufferSize = Math.floor(ctx.sampleRate * 0.4);
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }

        const noise = ctx.createBufferSource();
        noise.buffer = buffer;

        // Bandpass sweep rising up into open air
        const filter = ctx.createBiquadFilter();
        filter.type = "bandpass";
        filter.frequency.setValueAtTime(220, now);
        filter.frequency.exponentialRampToValueAtTime(1400, now + 0.35);

        const gain = ctx.createGain();
        gain.gain.setValueAtTime(0.28, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.4);

        noise.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        noise.start(now);
    } catch (e) {
        console.log("Audio notice:", e);
    }
}

// =========================================================================
// 5. AMBIENT BGM VIDEO AUDIO CONTROLLER
// =========================================================================
function initBgmController() {
    const video = document.getElementById("landingBgVideo");
    const toggleBtn = document.getElementById("bgmToggleBtn");
    const icon = document.getElementById("bgmIcon");
    const label = document.getElementById("bgmLabel");

    if (!video || !toggleBtn) return;

    toggleBtn.addEventListener("click", function () {
        if (video.muted) {
            video.muted = false;
            video.volume = 0.6;
            video.play().catch(err => console.log("Video audio play prevented:", err));
            toggleBtn.classList.add("playing");
            if (icon) icon.className = "bi bi-volume-up-fill text-success";
            if (label) label.textContent = "Ambience: On";
        } else {
            video.muted = true;
            toggleBtn.classList.remove("playing");
            if (icon) icon.className = "bi bi-volume-mute-fill";
            if (label) label.textContent = "Play Ambience";
        }
    });
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
