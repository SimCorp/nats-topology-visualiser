<template>
  <div id='zoom-bar'>
    <b-button id='zoom-bar-plus' @click="onPlus" variant="info">+</b-button>
    <b-button id='zoom-bar-minus' @click="onMinus" variant="info">-</b-button>
    <b-form-input id='zoom-bar-input' v-model="text" @input="onInput" placeholder="100%"></b-form-input>
  </div>
</template>

<script>
export default {
  data() {
    return {
      text: ''
    }
  },
  methods: {
    onPlus () {
      this.$emit('plusclick')
    },
    onMinus () {
      this.$emit('minusclick')
    },
    onInput () {
      this.$emit('input', this.text)
    }
  }
}
</script>

<style scoped>
#zoom-bar {
  text-align: center;
  position: absolute;
  bottom: 0;
  right: 0;
  margin-left: 20px;
  margin-bottom: 20px;
}
</style>
